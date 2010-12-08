using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Autofac;
using Autofac.Core;

namespace Orchard.Experimental {
    // note - not thread aware

    public interface IContainerSpyOutput {
        void Write(XElement target);
    }

    public class ContainerSpy : Module, IDependency {
        private readonly ConcurrentDictionary<int, ThreadContext> _contexts = new ConcurrentDictionary<int, ThreadContext>();

        protected override void Load(ContainerBuilder builder) {
            builder.RegisterInstance(new Output(_contexts)).As<IContainerSpyOutput>();

            //builder.RegisterCallback(cr => cr.Registered += (_, registered) => {
            //    registered.ComponentRegistration.Preparing += (__, preparing) => Preparing(GetContext(_contexts), preparing);
            //    registered.ComponentRegistration.Activating += (__, activating) => Activating(GetContext(_contexts), activating);
            //    registered.ComponentRegistration.Activated += (__, activated) => Activated(GetContext(_contexts), activated);
            //});
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration) {
            registration.Preparing += (__, preparing) => Preparing(GetContext(_contexts), preparing);
            registration.Activating += (__, activating) => Activating(GetContext(_contexts), activating);
            registration.Activated += (__, activated) => Activated(GetContext(_contexts), activated);
        }

        static ThreadContext GetContext(ConcurrentDictionary<int, ThreadContext> nodes) {
            return nodes.GetOrAdd(Thread.CurrentThread.ManagedThreadId, _ => {
                var tc = new ThreadContext();
                tc.Root = tc.Clock = tc.Chain = tc.Focus = new Node(tc);
                return tc;
            });
        }

        private static void Preparing(ThreadContext context, PreparingEventArgs preparing) {
            context.Focus = context.Focus.Preparing(preparing);
            context.Chain = context.Chain.Link(preparing, context.Focus);
            context.Clock = MoveClock(context.Clock, context.Focus);
        }

        private static void Activating(ThreadContext context, ActivatingEventArgs<object> activating) {
            context.Focus = context.Focus.Activating(activating);
        }

        private static void Activated(ThreadContext context, ActivatedEventArgs<object> activated) {
            context.Clock = MoveClock(context.Clock, context.Root);
            context.Chain = context.Chain.Activated(activated);
        }

        private static Node MoveClock(Node currentClock, Node newClock) {
            var scanEnable = newClock;
            while (scanEnable.Hot == false) {
                scanEnable.Hot = true;
                scanEnable = scanEnable._parent;
            }

            var scanDisable = currentClock;
            while (scanDisable != scanEnable) {
                scanDisable.Hot = false;
                scanDisable = scanDisable._parent;
            }
            return newClock;
        }




        class ThreadContext {
            public Node Root { get; set; }
            public Node Focus { get; set; }
            public Node Chain { get; set; }
            public Node Clock { get; set; }
        }

        class Node {
            private readonly ThreadContext _threadContext;
            public Node _parent;
            private Node _chain;
            public readonly IComponentRegistration _component;
            public readonly IDictionary<Guid, Node> _children = new Dictionary<Guid, Node>();

            public int _preparingCount;
            public int _activatingCount;
            public int _activatedCount;

            private bool _hot;
            readonly Stopwatch _stopwatch = new Stopwatch();
            private long _time;

            public Node(ThreadContext threadContext) {
                _threadContext = threadContext;
                _hot = true; // meta-nodes are hot to avoid any timing
            }

            public Node(Node parent, IComponentRegistration component) {
                _threadContext = parent._threadContext;
                _parent = parent;
                _component = component;
            }

            public bool Hot {
                get {
                    return _hot;
                }
                set {
                    if (_hot == value)
                        return;

                    _hot = value;
                    if (_hot) {
                        _stopwatch.Start();
                    }
                    else {
                        _stopwatch.Stop();
                    }
                }
            }

            public long Time {
                get { return _time+ _stopwatch.ElapsedTicks * 1000000 / Stopwatch.Frequency; }
            }
            public void AddTime(long time) { _time += time; }

            public override string ToString() {
                return _component == null ? "root" : _component.ToString();
            }


            private static void TraceMessage(string format, IComponentRegistration component) {
                //Trace.WriteLine(Message(format, component));
            }
            private static string Message(string format, IComponentRegistration component) {
                return string.Format(format, component.Id, string.Join(",", component.Services), Thread.CurrentThread.ManagedThreadId);
            }

            public Node Preparing(PreparingEventArgs e) {
                // move focus down a level on the tree
                // add a link in chain
                Node child;
                lock (_children) {
                    if (!_children.TryGetValue(e.Component.Id, out child)) {
                        child = new Node(this, e.Component);
                        _children[e.Component.Id] = child;
                    }
                }

                TraceMessage("Preparing[{2}] {0} {1}", e.Component);
                Interlocked.Increment(ref child._preparingCount);
                return child;
            }

            public Node Link(PreparingEventArgs e, Node focus) {
                if (focus._chain != null) {
                    TraceMessage("REACTIVATED: Preparing[{2}] {0} {1}", e.Component);
                }
                focus._chain = this;
                return focus;
            }

            public Node Activating(ActivatingEventArgs<object> e) {
                // move focus up a level on the tree
                if (_component == null) {
                    TraceMessage("UNMATCHED: Activating[{2}] {0} {1}", e.Component);
                    return this;
                }

                if (_component.Id != e.Component.Id) {
                    TraceMessage("MISSING: Activating[{2}] {0} {1}", _component);
                    return _parent.Activating(e);
                }

                TraceMessage("Activating[{2}] {0} {1}", e.Component);
                Interlocked.Increment(ref _activatingCount);
                return _parent;
            }

            public Node Activated(ActivatedEventArgs<object> e) {
                // remove a link in chain
                if (_component == null) {
                    TraceMessage("UNMATCHED: Activated[{2}] {0} {1}", e.Component);
                    return this;
                }

                if (_component.Id != e.Component.Id) {
                    _chain = _chain.Activated(e);
                    return this;
                }

                TraceMessage("Activated[{2}] {0} {1}", e.Component);
                Interlocked.Increment(ref _activatedCount);
                var chain = _chain;
                _chain = null;
                return chain;
            }

        }

        class Output : IContainerSpyOutput {
            private readonly ConcurrentDictionary<int, ThreadContext> _root;

            public Output(ConcurrentDictionary<int, ThreadContext> root) {
                _root = root;
            }

            public void Write(XElement target) {
                var elts = _root.Values
                    .Select(entry => Write(entry.Root))
                        .OrderByDescending(GetWeight)
                        .ToArray();

                var merged = _root.Values.Aggregate(new Node(null), (a, n) => Merge(a, n.Root));

                var totals = new TotalVisitor();
                totals.Visit(merged);

                target.Add(Write(merged));
                target.Add(Write(totals._totals));
                target.Add(elts);
            }

            private class TotalVisitor {
                public readonly Node _totals = new Node(null);

                public void Visit(Node source) {
                    foreach (var child in source._children) {
                        Visit(child.Key, child.Value);
                    }
                }

                public void Visit(Guid key, Node source) {
                    Node target;
                    if (!_totals._children.TryGetValue(key, out target)) {
                        target = new Node(_totals, source._component);
                        _totals._children[key] = target;
                    }
                    target._preparingCount += source._preparingCount;
                    target._activatingCount += source._activatingCount;
                    target._activatedCount += source._activatedCount;
                    foreach (var child in source._children) {
                        Visit(child.Key, child.Value);
                    }
                }

            }

            private static Node Merge(Node target, Node source) {
                target._preparingCount += source._preparingCount;
                target._activatingCount += source._activatingCount;
                target._activatedCount += source._activatedCount;
                target.AddTime(source.Time);
                foreach (var sourceChild in source._children) {
                    Node targetChild;
                    if (!target._children.TryGetValue(sourceChild.Key, out targetChild)) {
                        targetChild = new Node(target, sourceChild.Value._component);
                        target._children[sourceChild.Key] = targetChild;
                    }
                    Merge(targetChild, sourceChild.Value);
                }
                return target;
            }

            private static XElement Write(Node node) {
                var elt = new XElement(
                    "Component",
                    new XAttribute("services", node._component != null ? string.Join(",", node._component.Services) : "root"),
                    new XAttribute("preparing", node._preparingCount),
                    new XAttribute("activating", node._activatingCount),
                    new XAttribute("activated", node._activatedCount));

                lock (node._children) {
                    var elts = node._children.Values
                        .Select(Write)
                        .OrderByDescending(GetMicrosecondInclusive)
                        .ToArray();
                    elt.Add(elts);

                    var weight = elts.Aggregate(node._preparingCount, (a, e) => a + GetWeight(e));

                    elt.SetAttributeValue("weight", weight);
                    elt.SetAttributeValue("usinc", node.Time);
                    if (weight != 0)
                        elt.SetAttributeValue("usincper", node.Time / weight);
                }

                return elt;
            }

            private static long GetMicrosecondInclusive(XElement elt) {
                var attr = elt.Attribute("usinc");
                return attr == null ? 0 : long.Parse(attr.Value);
            }
            private static int GetWeight(XElement elt) {
                var attr = elt.Attribute("weight");
                return attr == null ? 0 : int.Parse(attr.Value);
            }
        }
    }
}
