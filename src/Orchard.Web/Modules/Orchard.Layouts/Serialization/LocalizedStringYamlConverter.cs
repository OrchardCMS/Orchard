using System;
using Orchard.Localization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Orchard.Layouts.Serialization {
    public class LocalizedStringYamlConverter : IYamlTypeConverter {
        public bool Accepts(Type type) => type == typeof(LocalizedString);

        public object ReadYaml(IParser parser, Type type) => new LocalizedString(parser.Expect<Scalar>()?.Value);

        public void WriteYaml(IEmitter emitter, object value, Type type) =>
            emitter.Emit(new Scalar((value as LocalizedString)?.Text ?? ""));
    }
}