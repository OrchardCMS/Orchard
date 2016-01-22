﻿using System;
using Orchard.Caching;
using Orchard.Events;

namespace Orchard.DisplayManagement.Descriptors {

    public interface IShapeTableManager : ISingletonDependency {
        ShapeTable GetShapeTable(string themeName);
    }

    public interface IShapeTableProvider : IDependency {
        void Discover(ShapeTableBuilder builder);
    }

    public interface IShapeTableEventHandler : IEventHandler {
        void ShapeTableCreated(ShapeTable shapeTable);
    }

}
