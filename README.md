# Library Mode for iOS & Android Sample Repo

This repo contains a sample illustrating how to generate shared or static libraries that can call .NET functions from native code. 

## Prepare Your System

### Install nightly dotnet sdk

Grab a .NET 9 preview and install it

https://github.com/dotnet/installer#table

### Install the workload

Install the workload that contains the tooling to support library mode for iOS and Android via:

`dotnet workload install mobile-librarybuilder`

### Install the Android SDK / NDK

If you don't have it installed already, follow https://github.com/dotnet/runtime/blob/main/docs/workflow/testing/libraries/testing-android.md 

.NET uses NDK r23c. Make sure to set these environment variables as they are used when building libraries:

`ANDROID_NDK_ROOT`
`ANDROID_SDK_ROOT`

### Install XCode and XCode build tools

The latest XCode can be downloaded from https://developer.apple.com

## Building the Libraries

### Create a Project

Since this feature is not integrated with the .NET Android and iOS SDK's, the project you need to create is rather simple.  

```
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net8.0</TargetFramework>    
    <SelfContained>true</SelfContained>
    <NativeLib>shared</NativeLib>
  </PropertyGroup>

  <!-- Your files here --> 
```

Similar to NativeAOT, the key property to turn this into a native library is `<NativeLib>true</NativeLib>`.

### Add Linker Descriptor to Preserve UnManagedCallerOnly attributes

The .NET IL linker does not yet have the ability to preserve methods decorated with `[UnmanagedCallersOnly]` attributes.  To tell the linker what to hold onto, you have to create an ILLink.Descriptor xml file and then add it to your project.  

```
<linker>
  <assembly fullname="<ASSEMBLY_NAME>">
    <type fullname="<FULL_CLASS_NAME>">
      <method name="<METHOD>" />
    </type>
  </assembly>
</linker>
```

```
<ItemGroup>
    <!-- Preserves the UnmanagedCallersOnly method -->
    <TrimmerRootDescriptor Include="$(MSBuildThisFileDirectory)ILLink.Descriptor.xml" />
</ItemGroup>
```

### Build the Project

iOS and Android are self-contained configurations and therefore, `dotnet publish` needs to be utilized.  There are a few additional properties to pass in order to fully activate library mode:

*iOS*

`dotnet publish -r ios-arm64 /p:RunAOTCompilation=true`

*Android*

`dotnet publish -r android-arm64|arm|x64|x86 /p:RunAOTCompilation=true /p:ForceFullAOT=true`

## Packaging and Using the Libraries

We currently do not bundle the managed assemblies into the library like nativeaot does. It's unclear whether we will add better support.

### Assemblies

The assemblies used to build the library are located in `bin/<Configuration>/net<version>/<ios|android>-<architecture>/publish`. Add them to your native project and make sure they are accessible on disk. By default, they are not directly accessible on Android, so you will need to do something such as zip the contents and then unzip at runtime.

They can be in any location on disk, but you need to tell the runtime where they are located by setting the `DOTNET_LIBRARY_ASSEMBLY_PATH` environment variable.

### Using the Library

Since it's a regular native library, it can be added to your native project just like any other. The library will have public symbols that map to the `EntryPoint` value for each method decorated with `[UnmanagedCallersOnly(EntryPoint = "<your_method_here>")]`.  You can simply call the function from anywhere in your native project and it should just work.
