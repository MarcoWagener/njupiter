﻿using System;
using System.Xml;

using FakeItEasy;

using nJupiter.Configuration;
using nJupiter.DataAccess.Users;
using nJupiter.DataAccess.Users.Caching;

using NUnit.Framework;

namespace nJupiter.Tests.DataAccess.Users {
	
	[TestFixture]
	public class UserRepositoryManagerTests {

		[Test]
		public void GetRepository_CreateDefaultInstanceFromConfig_ReturnsRepositorySetToDefault() {
			UserRepositoryManager userRepositoryManager = GetUserRepositoryManager();
			var repository = userRepositoryManager.GetRepository();
			Assert.AreEqual("SQLRepository", repository.Name);
		}

		[Test]
		public void GetRepository_GetInstanceByName_ReturnsInstance() {
			UserRepositoryManager userRepositoryManager = GetUserRepositoryManager();
			var repository = userRepositoryManager.GetRepository("TestRepository");
			Assert.IsTrue(repository is UserRepositoryAdapter);
		}

		[Test]
		public void GetRepository_GetInstanceWithoutCacheConfigured_ReturnsInstanceWithGenericCache() {
			UserRepositoryManager userRepositoryManager = GetUserRepositoryManager();
			var repository = userRepositoryManager.GetRepository("TestRepository") as UserRepositoryBase;
			Assert.IsTrue(repository.UserCache is GenericUserCache);
		}

		[Test]
		public void GetRepository_GetInstanceWithCacheConfigured_ReturnsInstanceWithCorrectCache() {
			UserRepositoryManager userRepositoryManager = GetUserRepositoryManager();
			var repository = userRepositoryManager.GetRepository("SQLRepository") as UserRepositoryBase;
			Assert.IsTrue(repository.UserCache is HttpRuntimeUserCache);
		}


		[Test]
		public void GetRepository_PassingEmptyString_ReturnsDefaultContext() {
			UserRepositoryManager userRepositoryManager = GetUserRepositoryManager();
			var defaultRepository = userRepositoryManager.GetRepository();
			var repository = userRepositoryManager.GetRepository(string.Empty);
			Assert.AreEqual(defaultRepository, repository);
		}

		[Test]
		public void GetRepository_GetRepositoryNonExistingCache_ThrowsApplicationException() {
			UserRepositoryManager userRepositoryManager = GetUserRepositoryManager();
			Assert.Throws<ApplicationException>(() => userRepositoryManager.GetRepository("RepositoryNonExistingCache"));
		}


		[Test]
		public void GetRepository_GetNonExistingInstance_ThrowsApplicationException() {
			UserRepositoryManager userRepositoryManager = GetUserRepositoryManager();
			Assert.Throws<ApplicationException>(() => userRepositoryManager.GetRepository("NonExistingRepository"));

		}

		[Test]
		public void GetRepository_GetDefaultInstanceTwice_ReturnsSameInstance() {
			UserRepositoryManager userRepositoryManager = GetUserRepositoryManager();
			var repository1 = userRepositoryManager.GetRepository();
			var repository2 = userRepositoryManager.GetRepository();
			Assert.AreSame(repository1, repository2);
		}

		[Test]
		public void GetRepository_DiscardConfig_ReturnsNewInstanceOfRepository() {
			var configHandler = A.Fake<IConfigHandler>();
			var config = GetConfig();
			A.CallTo(() => configHandler.GetConfig()).Returns(config);
			var repositoryFactory = new UserRepositoryManager(configHandler);
			var repositoryBeforeDiscardedConfig = repositoryFactory.GetRepository();
			config.Discard();
			var repositoryAfterDiscardedConfig = repositoryFactory.GetRepository();
			Assert.AreNotSame(repositoryBeforeDiscardedConfig, repositoryAfterDiscardedConfig);

		}


		private static UserRepositoryManager GetUserRepositoryManager() {
			var configHandler = A.Fake<IConfigHandler>();
			var config = GetConfig();
			A.CallTo(() => configHandler.GetConfig()).Returns(config);
			return new UserRepositoryManager(configHandler);
		}

		private static IConfig GetConfig() {
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(string.Format(testConfig));
			return ConfigFactory.Create("testConfig", xmlDocument.DocumentElement);
		}

		private const string testConfig =
			@"<configuration>
				<userRepositories>
					<userRepository name=""SQLRepository"" default=""true"">
						<userRepositoryFactory
							qualifiedTypeName=""nJupiter.DataAccess.Users.Sql.UserRepositoryFactory, nJupiter.DataAccess.Users.Sql""/>
						<settings>
							<dataSource value=""SQLAdapter"" />
							<hashPassword value=""true"" />
							<cache>
								<userCacheFactory
									qualifiedTypeName=""nJupiter.DataAccess.Users.Caching.HttpRuntimeUserCacheFactory, nJupiter.DataAccess.Users""/>
								<minutesInCache value=""60"" />
							</cache>
							<predefinedProperties>
								<firstName value=""firstName"" />
								<lastName value=""lastName"" />
							</predefinedProperties>
						</settings>
					</userRepository>
					<userRepository name=""TestRepository"">
						<userRepositoryFactory
							qualifiedTypeName=""nJupiter.Tests.DataAccess.Users.UserRepositoryAdapter, nJupiter.Tests""/>
						<settings>
							<someSettings value=""test"" />
						</settings>
					</userRepository>
					<userRepository name=""RepositoryNonExistingCache"">
						<userRepositoryFactory
							qualifiedTypeName=""nJupiter.Tests.DataAccess.Users.UserRepositoryAdapter, nJupiter.Tests""/>
						<settings>
							<cache>
								<userCacheFactory
									qualifiedTypeName=""nJupiter.DataAccess.Users.Caching.NonExistingCache, nJupiter.DataAccess.Users""/>
							</cache>
						</settings>
					</userRepository>
				</userRepositories>
			</configuration>";

	}
}