## what to do 
        - Code before migration

        - Code after migration

'# Motif to MS Test library migration
This article shows how to migrate from motif to MS test framework

## Namespace reference removal
As first part of UT.sln migration, need to remove the usage of motif related artifacts and replace with MS Test related artifacts

- Code before migration:

        using MS.Internal.Motif.Runtime;
        using MS.Internal.Motif.TestClasses;
        using MS.Internal.Motif.VSTS.Framework;
		using MS.Internal.Motif.Runtime.TestAttributes;
		
- Code after migration:

        using Microsoft.VisualStudio.TestTools.UnitTesting;

## Annotation reference removal
Remove references of Motif library from code such as [MS.Internal.Motif.Runtime.TestAttributes.TestClass]		
        
## Removal of OfficeUnitTestClass inheritance
- Code before migration:
        [GeneratedCode("unit tests", "1.0")]
        public abstract class BaseTestClass : OfficeUnitTestClass
        {
                ...
        }

- Code after migration:
        [GeneratedCode("unit tests", "1.0")]
        public abstract class BaseTestClass 
        {
                ...
        }
		
## Removal of TestClass inheritance

- Code before migration:
        public sealed class ContinuousReplicationTestClass : TestClass
        {
                ...
        }

- Code after migration:
        
        public sealed class ContinuousReplicationTestClass 
        {
                ...
        }
		
## Removal of Setup and Teardown from TestMethod
- Code before migration:
         [TestMethod("Setup", "Teardown")]

- Code after migration:
        [TestMethod]