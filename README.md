# releaseoss

A command-line tool to pack and publish releases of open source projects as automatically as possible.

## Motivation

The motivation behind `releaseoss` is to create a simple utility to pack release files of my open source projects and push them to the respective open source websites that serve as download locations.

I once started having my [NAnt](http://nant.sourceforge.net/) build scripts perform these actions.
Soon, the build scripts became [complex enough](https://sourceforge.net/p/pathdefenselib/code/ci/9477acbfa169599253beb06dfd5936794f1d4610/tree/release.build) to warrant a project of their own.
In [their own project](https://github.com/fhaag/DeploymentScriptGenerator), I became aware that I'd need so much utility code to modify or generate the appropriate NAnt scripts per project that I could just as well drop the NAnt dependency in the build process and stick with a custom command line utility.

## Design Goals

This tool is meant to simplify packing and publishing releases of open source projects.
With this said, please be aware of the following points that are specific to this project:

- This tool is *not* a build system. It is *not* meant to be as flexible as possible. It assumes that OSS projects stick to a certain set of (somewhat arbitrary) conventions in aspects such as directory structure.
- This tool cannot process or target *virtually everything*. It is designed to work with open source projects written using one of a small number of technologies, and can publish the releases to a small number of websites.

## Usage

The general idea is that you place a small [JSON](https://www.json.org/) file named `ossrelease.json` in the root folder of your project.
When you call the `releaseoss` tool in that directory, it will read some settings from the `ossrelease.json` file.
Other than that, you just need to specify a command and possibly a target version number for your release.