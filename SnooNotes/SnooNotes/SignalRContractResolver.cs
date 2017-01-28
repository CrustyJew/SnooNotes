﻿using System;
using System.Reflection;
using Microsoft.AspNetCore.SignalR.Infrastructure;
using Newtonsoft.Json.Serialization;

namespace SnooNotes
{
    public class SignalRContractResolver : IContractResolver {
        private readonly Assembly _assembly;
        private readonly IContractResolver _camelCaseContractResolver;
        private readonly IContractResolver _defaultContractSerializer;

        public SignalRContractResolver() {
            _defaultContractSerializer = new DefaultContractResolver();
            _camelCaseContractResolver = new CamelCasePropertyNamesContractResolver();
            _assembly = typeof( Connection ).GetTypeInfo().Assembly;
        }


        public JsonContract ResolveContract( Type type ) {
            if ( type.GetTypeInfo().Assembly.Equals( _assembly ) )
                return _defaultContractSerializer.ResolveContract( type );

            return _camelCaseContractResolver.ResolveContract( type );
        }

    }
}
