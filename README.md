
## About The Project

MotionSim is a Derail Valley mod that adds motion sim support.

## Building
1. Obtain a local copy of the repository
2. Setup your build targets
	1. Make a copy of `Directory.Build.targets.EXAMPLE`
	2. Rename to `Directory.Build.targets`
	3. Open `Directory.Build.targets` in your favourite text editor and update the paths:
		- `DVInstallDir` - location Derail Valley is installed to
		- `UnityInstallDir` - location of UnityEngine Editor (if installed)
		- `SignToolPath` - optional and can be left blank if you are not using code signing
		- `Cert-Thumb` - can also be left blank if you are not using code signing, otherwise it needs to be the thumbprint of your cetificate
4) Open the 'MotionSim.sln' in VisualStudio 2022 (community edition is sufficient)
5) Build all - this may take a couple of build the first time, YMMV
