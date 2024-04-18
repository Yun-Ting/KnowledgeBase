'# csproj sdk migration
This article shows how to multitarget a csharp project file


## Header version tag removal
As first part of SDK migration, need to remove the xml version and encoding attribute from the header

Assuming the following is format of csproj file:
<?xml version="1.0" encoding="utf-8"?>
<Project ToolsVersion="14.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
</Project>
- Code before migration:

        <?xml version="1.0" encoding="utf-8"?>
        <Project ToolsVersion="14.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">

- Code after migration:

	<Project Sdk="Office.Build.Before;Microsoft.NET.Sdk;Office.Build.After">

## Header version tag removal
Adding attributes for supporting multitargeting and setting flase for generating default framework attribute. Also adding sdkmid.props

- Code before migration:
  <PropertyGroup>
    <OutputType>Library</OutputType>
    <AssemblyName>...</AssemblyName>
  </PropertyGroup>

- Code after migration:
  <PropertyGroup>
    <AppendTargetFrameworkToOutputPath Condition=" '$(TargetFramework)' == 'net472' ">false</AppendTargetFrameworkToOutputPath>
    <TargetFrameworks>net472;$(DotNetCoreTarget)</TargetFrameworks>
    <OutputType>Library</OutputType>
    <AssemblyName>...</AssemblyName>
    <GenerateAssemblyInfo>false</GenerateAssemblyInfo>
    <GenerateTargetFrameworkAttribute>false</GenerateTargetFrameworkAttribute>
  </PropertyGroup>
  <Import Project="$(OTOOLS)/inc/msbuild/sdkmid.props" />

