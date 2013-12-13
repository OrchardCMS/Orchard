namespace Orchard.Messaging.Models {
    public class MessageRecipient {
        public string AddressOrAlias { get; set; }

        public MessageRecipient() {}

        public MessageRecipient(string addressOrAlias) {
            AddressOrAlias = addressOrAlias;
        }

        public override string ToString() {
            return AddressOrAlias;
        }
    }
}