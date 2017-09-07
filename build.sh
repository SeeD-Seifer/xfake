#!/bin/bash

nuget install FAKE -Version 4.63.0 -o packages
mono --runtime=v4.0 packages/FAKE.4.63.0/tools/FAKE.exe xfake/build.fsx $@
