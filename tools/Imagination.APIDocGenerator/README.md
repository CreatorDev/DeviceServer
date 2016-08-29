# API documentation generator
----

This tool generates documentation for the LWM2M device management server REST API by using reflection to gather assemblies 
from an Imagination.DeviceServer executable, reading metadata from any class that extends ControllerBase and serialising 
a filtered subset of required metadata into requested output formats.

This tool currently supports RAML 0.8 serialisation.

##  Getting started

Run the following command for help on how to run the tool:

`Imagination.APIDocGenerator -h` 

Refer to **appsettings.json** for tool configuration settings.

_Note: In order to use this tool, the Device Server must first be built._

## Development tasks

- Implement RAML 1.0 serialiser
- Implement Swagger YAML and JSON serialisers