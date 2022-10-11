# 1. The executables
The executables include two parts, one for the Charon compiler and the other for the Charon fuzzer.

## 1.1 The Charon compiler

The Charon compiler is a modified wrapper of the clang and the afl-clang-fast compiler. It is used for collecting run-time coverage of Charon fuzzer.

### Requirements:
- clang (version 12.0.0+)

### Usage:

Replace the original compiler (`clang` or `gcc`) with `charon-compiler` (`charon-compiler++`) when building a program, e.g.:

a. autoconf and make:

```shell
CC=/path-to-compiler/charon-compiler CXX=/path-to-compiler/charon-compiler++ ./configure [...options...]  # change complier
```

b. cmake:

```shell
cmake -DCMAKE_C_COMPILER=/path-to-compiler/charon-compiler -DCMAKE_CXX_COMPILER=/path-to-compiler/charon-compiler++ [..options..] ../src    # change complier
```

### Files:
- charon-compiler
- charon-compiler++


## 1.2 The Charon fuzzer

The implementation of Charon fuzzer, designed for ICS protocols.


### Requirements:
- build-essential
- automake
- libtool
- libc6-dev-i386
- python-pip
- g++-multilib
- mono-complete
- python-software-properties
- software-properties-common

### Usage:

#### a. Create a shared memory

```shell
cd /dev/shm
dd if=/dev/zero bs=10M count=1 of=$name-of-shared-memory
```

**Hint**: `$name-of-shared-memeory` should be replaced by any name you like.

####  b. Fuzzing

```shell
export LD_LIBRARY_PATH=/path-to-charon/:$LD_LIBRARY_PATH
export SHM_ENV_VAR=/dev/shm/$name-of-shared-memory
mono /path-to-charon/charon.exe /path-to-charon/HelloWorld.xml
```

**Hint**: `/path-to-charon/HelloWorld.xml` is a simple demo of Pits and it should be replaced by the Pits (data models and state model) file of the target ICS protocol implementation.


### Files:
Charon command line program:
- charon.exe

Charon Engine (based on Peach's framework):
- Charon.Core.dll
- Charon.Core.OS.Linux.dll
- Charon.Core.Test.dll
- Charon.Core.Test.OS.Linux.dll
- libcharonControl.so

Thirdparty dependence:
- NLog.dll
- Ionic.Zip.dll
- nunit.framework.dll
- Renci.SshNet.dll
- SharpPcap.dll
- ZeroMQ.dll
- PacketDotNet.dll
- IronPython.StdLib.zip