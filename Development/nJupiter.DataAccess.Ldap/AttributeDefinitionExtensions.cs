using System;
using System.Collections.Generic;
using System.Linq;

namespace nJupiter.DataAccess.Ldap {
	internal static class AttributeDefinitionExtensions {
		public static void Attach(this IList<IAttributeDefinition> attributes, string attributeName, bool excludeFromNameSearch) {
			if(!string.IsNullOrEmpty(attributeName)) {
				var definition = attributes.FirstOrDefault(a => string.Equals(a.Name, attributeName, StringComparison.InvariantCultureIgnoreCase));
				if(definition != null) {
					if(definition.ExcludeFromNameSearch == excludeFromNameSearch) {
						return;
					}
					attributes.Remove(definition);
				}
				definition = new AttributeDefinition(attributeName, excludeFromNameSearch);
				attributes.Add(definition);
			}			
		}		
	}
}