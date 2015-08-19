#!/bin/bash

nuget install FAKE -Version 4.0.3 -o packages
mono --runtime=v4.0 packages/FAKE.4.0.3/tools/FAKE.exe xfake/build.fsx $@
