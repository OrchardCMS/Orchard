﻿namespace Orchard.Workflows.Services {
    public interface ISignalService : IDependency {
        string CreateNonce(int contentItemId, string signal);
        bool DecryptNonce(string nonce, out int contentItemId, out string signal);
    }
}