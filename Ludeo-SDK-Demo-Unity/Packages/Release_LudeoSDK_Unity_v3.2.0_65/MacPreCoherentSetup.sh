#!/bin/bash

# Remove extended attributes from the Coherent Unity3DPlugin.bundle
xattr -cr Coherent/Plugins/MacOSX/cohtml_Unity3DPlugin.bundle

# Remove extended attributes from the Coherent Unity3DPlugin.IL2CPP.bundle
xattr -cr Coherent/Plugins/MacOSX/cohtml_Unity3DPlugin.IL2CPP.bundle