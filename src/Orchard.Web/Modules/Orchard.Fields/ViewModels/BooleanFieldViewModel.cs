namespace Orchard.Fields.ViewModels {

    public class BooleanFieldViewModel {

        public string Name { get; set; }

        public bool? Value { get; set; }

        public string NotSetLabel { get; set; }
        public string OnLabel { get; set; }
        public string OffLabel { get; set; }
    }
}