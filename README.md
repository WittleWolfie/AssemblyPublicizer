﻿# AssemblyPublicizer

AssemblyPublicizer is an MSBuild task that creates copies of assemblies in which all members are public.

This is useful for easily referencing non-public members without the hassles or associated performance hits of reflection.

It addresses one key issue with other publicizers in that, it avoids publicizing explicit implementations, which sometimes causes issues with Jetbrains Rider's intellisense.

# Usage

1. Install the Nuget package
2. Create the MSBuild Target. Properties are:  
   `InputAssemblies`: Assemblies to be publicized.  
   `OutputDir`: Output directory to store the publicized assemblies.  
   `PublicizeExplicitImpls`: Whether or not to publicize explicit implementation. (False by default).