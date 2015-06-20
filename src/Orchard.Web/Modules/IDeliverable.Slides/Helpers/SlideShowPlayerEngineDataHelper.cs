using System;
using System.Linq.Expressions;
using IDeliverable.Slides.Services;
using Orchard.ContentManagement;
using Orchard.Layouts.Helpers;
using Orchard.Utility;

namespace IDeliverable.Slides.Helpers
{
    public static class SlideshowPlayerEngineDataHelper
    {
        public static TProperty Retrieve<TEngine, TProperty>(this TEngine engine, Expression<Func<TEngine, TProperty>> targetExpression, Func<TProperty> defaultValue = null) where TEngine : ISlideshowPlayerEngine
        {
            var propertyInfo = ReflectionHelper<TEngine>.GetPropertyInfo(targetExpression);
            var name = propertyInfo.Name;
            var data = engine.Data;
            var value = data.Get(name);

            return !String.IsNullOrWhiteSpace(value) ? XmlHelper.Parse<TProperty>(value) : defaultValue != null ? defaultValue() : default(TProperty);
        }

        public static TProperty Retrieve<TProperty>(this SlideshowPlayerEngine engine, string name, Func<TProperty> defaultValue = null)
        {
            var data = engine.Data;
            var value = data.Get(name);
            return !String.IsNullOrWhiteSpace(value) ? XmlHelper.Parse<TProperty>(value) : defaultValue != null ? defaultValue() : default(TProperty);
        }

        public static void Store<TEngine, TProperty>(this TEngine engine, Expression<Func<TEngine, TProperty>> targetExpression, TProperty value) where TEngine : ISlideshowPlayerEngine
        {
            var propertyInfo = ReflectionHelper<TEngine>.GetPropertyInfo(targetExpression);
            var name = propertyInfo.Name;

            Store(engine, name, value);
        }

        public static void Store<TProperty>(this ISlideshowPlayerEngine engine, string name, TProperty value)
        {
            engine.Data[name] = XmlHelper.ToString(value);
        }
    }
}