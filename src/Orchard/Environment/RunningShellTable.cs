using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Orchard.Environment.Configuration;

namespace Orchard.Environment {
    public interface IRunningShellTable {
        void Add(ShellSettings settings);
        void Remove(ShellSettings settings);
        void Update(ShellSettings settings);
        ShellSettings Match(HttpContextBase httpContext);
        ShellSettings Match(string host, string appRelativeCurrentExecutionFilePath);
    }

    public class RunningShellTable : IRunningShellTable {
        private IEnumerable<ShellSettings> _shells = Enumerable.Empty<ShellSettings>();
        private IDictionary<string, IEnumerable<ShellSettings>> _shellsByHost;
        private readonly ConcurrentDictionary<string, ShellSettings> _shellsByHostAndPrefix = new ConcurrentDictionary<string, ShellSettings>(StringComparer.OrdinalIgnoreCase);

        private ShellSettings _fallback;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public void Add(ShellSettings settings) {
            _lock.EnterWriteLock();
            try {
                _shells = _shells
                    .Where(s => s.Name != settings.Name)
                    .Concat(new[] {settings})
                    .ToArray();

                Organize();
            }
            finally {
                _lock.ExitWriteLock();
            }
        }

        public void Remove(ShellSettings settings) {
            _lock.EnterWriteLock();
            try {
                _shells = _shells
                    .Where(s => s.Name != settings.Name)
                    .ToArray();

                Organize();
            }
            finally {
                _lock.ExitWriteLock();
            }
        }

        public void Update(ShellSettings settings) {
            _lock.EnterWriteLock();
            try {
                _shells = _shells
                    .Where(s => s.Name != settings.Name)
                    .ToArray();

                _shells = _shells
                    .Concat(new[] {settings})
                    .ToArray();

                Organize();
            }
            finally {
                _lock.ExitWriteLock();
            }
        }

        private void Organize() {
            var qualified =
                _shells.Where(x => !string.IsNullOrEmpty(x.RequestUrlHost) || !string.IsNullOrEmpty(x.RequestUrlPrefix));

            var unqualified = _shells
                .Where(x => string.IsNullOrEmpty(x.RequestUrlHost) && string.IsNullOrEmpty(x.RequestUrlPrefix))
                .ToList();

            _shellsByHost = qualified
                .SelectMany(s => s.RequestUrlHost == null || s.RequestUrlHost.IndexOf(',') == -1 ? new[] {s} : 
                    s.RequestUrlHost.Split(new [] {','}, StringSplitOptions.RemoveEmptyEntries)
                     .Select(h => new ShellSettings(s) {RequestUrlHost = h}))
                .GroupBy(s => s.RequestUrlHost ?? string.Empty)
                .OrderByDescending(g => g.Key.Length)
                .ToDictionary(x => x.Key, x => x.AsEnumerable(), StringComparer.OrdinalIgnoreCase);

            if (unqualified.Count() == 1) {
                // only one shell had no request url criteria
                _fallback = unqualified.Single();
            }
            else if (unqualified.Any()) {
                // two or more shells had no request criteria. 
                // this is technically a misconfiguration - so fallback to the default shell
                // if it's one which will catch all requests
                _fallback = unqualified.SingleOrDefault(x => x.Name == ShellSettings.DefaultName);
            }
            else {
                // no shells are unqualified - a request that does not match a shell's spec
                // will not be mapped to routes coming from orchard
                _fallback = null;
            }

            _shellsByHostAndPrefix.Clear();
        }

        public ShellSettings Match(HttpContextBase httpContext) {
            // use Host header to prevent proxy alteration of the orignal request
            try {
                var httpRequest = httpContext.Request;
                if (httpRequest == null) {
                    return null;
                }

                var host = httpRequest.Headers["Host"];
                var appRelativeCurrentExecutionFilePath = httpRequest.AppRelativeCurrentExecutionFilePath;

                return Match(host ?? string.Empty, appRelativeCurrentExecutionFilePath);
            }
            catch(HttpException) {
                // can happen on cloud service for an unknown reason
                return null;
            }
        }

        public ShellSettings Match(string host, string appRelativePath) {
            _lock.EnterReadLock();
            try {
                if (_shellsByHost == null) {
                    return null;
                }

                // optimized path when only one tenant (Default), configured with no custom host
                if (!_shellsByHost.Any() && _fallback != null) {
                    return _fallback;
                }

                // removing the port from the host
                var hostLength = host.IndexOf(':');
                if (hostLength != -1) {
                    host = host.Substring(0, hostLength);
                }

                string hostAndPrefix = host + "/" + appRelativePath.Split('/')[1];

                return _shellsByHostAndPrefix.GetOrAdd(hostAndPrefix, key => {
                    
                    // filtering shells by host
                    IEnumerable<ShellSettings> shells;

                    if (!_shellsByHost.TryGetValue(host, out shells)) {
                        if (!_shellsByHost.TryGetValue("", out shells)) {

                            // no specific match, then look for star mapping
                            var subHostKey = _shellsByHost.Keys.FirstOrDefault(x =>
                                x.StartsWith("*.") && host.EndsWith(x.Substring(2))
                                );

                            if (subHostKey == null) {
                                return _fallback; 
                            }

                            shells = _shellsByHost[subHostKey];
                        }
                    }
                    
                    // looking for a request url prefix match
                    var mostQualifiedMatch = shells.FirstOrDefault(settings => {
                        if (settings.State == TenantState.Disabled) {
                            return false;
                        }

                        if (String.IsNullOrWhiteSpace(settings.RequestUrlPrefix)) {
                            return true;
                        }

                        return key.Equals(host + "/" + settings.RequestUrlPrefix, StringComparison.OrdinalIgnoreCase);
                    });

                    return mostQualifiedMatch ?? _fallback;
                });
                
            }
            finally {
                _lock.ExitReadLock();
            }
        }
    }
}
