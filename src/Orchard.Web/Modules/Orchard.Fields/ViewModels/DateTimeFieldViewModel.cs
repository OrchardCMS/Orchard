namespace Orchard.Fields.ViewModels {

    public class DateTimeFieldViewModel {

        public string Name { get; set; }

        public string Date { get; set; }
        public string Time { get; set; }

        public bool ShowDate { get; set; }
        public bool ShowTime { get; set; }

        public string Hint { get; set; }
        public bool Required { get; set; }
    }
}