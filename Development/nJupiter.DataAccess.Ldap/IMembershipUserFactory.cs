using System.Collections.Generic;
using System.Web.Security;

using nJupiter.DataAccess.Ldap.DirectoryServices.Abstractions;

namespace nJupiter.DataAccess.Ldap {
	internal interface IMembershipUserFactory {
		MembershipUser Create(IEntry entry);
		MembershipUserCollection CreateCollection(IEnumerable<IEntry> entries);
	}
}