namespace Orchard.Environment.Configuration {
    public class TenantState {
        public TenantState(string state) {
            switch (state) {
                case "Uninitialized":
                    CurrentState = State.Uninitialized;
                    break;
                case "Running":
                    CurrentState = State.Running;
                    break;
                case "Disabled":
                    CurrentState = State.Disabled;
                    break;
                default:
                    CurrentState = State.Invalid;
                    break;
            }
        }

        public State CurrentState { get; set; }

        public enum State {
            Uninitialized,
            Running,
            Disabled,
            Invalid
        }

        public override string ToString() {
            switch (CurrentState) {
                case State.Uninitialized:
                    return "Uninitialized";
                case State.Running:
                    return "Running";
                case State.Disabled:
                    return "Disabled";
            }
            return "Invalid";
        }
    }
}
