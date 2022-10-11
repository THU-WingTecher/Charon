# Demo Source Files of Charon
## Prerequisites

Install automake, mono package and some required packages

```shell
sudo apt-get install build-essential automake libtool libc6-dev-i386 python-pip g++-multilib mono-complete python-software-properties software-properties-common
```



Install gcc-4.4 and g++-4.4 (as Pin component in Charon has a compilation issue with newer version of gcc like gcc-5.4)

First add apt repository:

```shell
sudo add-apt-repository ppa:ubuntu-toolchain-r/test
```

If you find that it just didn't work on your linux distribution and got failed at installing gcc/g++ 4.4 follow the commands below :

First edit the sources.list file:

```shell
sudo vim /etc/apt/sources.list
```

then add the following line to your sources.list file :

```shell
deb  http://dk.archive.ubuntu.com/ubuntu/  trusty  main  universe
```



Now:

```shell
sudo apt-get update
sudo apt install gcc-4.4
sudo apt install g++-4.4
```



## Build

```shell
clang control.c -fPIC -shared -o libcharonControl.so
CC=gcc-4.4 CXX=g++-4.4 ./waf configure
CC=gcc-4.4 CXX=g++-4.4 ./waf install
```

Setup environment variables:

append the following entries in the shell configuration file (`~/.bashrc`).

```shell
export PATH=/path-to-Charon/:$PATH
export LD_LIBRARY_PATH=/path-to-Charon/:$LD_LIBRARY_PATH
```

**or** execute the following shell (**not always safe!**):

```shell
bash setup_env.sh
```



# Running

## Create shared memory

```shell
cd /dev/shm
dd if=/dev/zero bs=10M count=1 of=$name-of-shared-memory
```

**Hint**: `$name-of-shared-memeory` should be replaced by any name you like.

##  Fuzzing

```shell
export SHM_ENV_VAR=/dev/shm/$name-of-shared-memory
mono /path-to-Charon/output/linux_x86_64_release/bin/Charon.exe /path-to-Charon/output/linux_x86_64_release/bin/samples/HelloWorld.xml
```





# Usage

Charon* adds several more options to Charon, all of these option **are not required**:

-pro: use Charon\*;

-pathp=$file-name: write path log to `file-name`;

-pathb=$file-name: write branch log to `file-name`;

-asanLog=$directory: save all the asan reports to `directory`;

-repro=$crash-directory: `directory` used to reproduce crash, for example, `./Logs-new/cyclone_test_2.xml_Default_20200408123702/Faults/ProcessExitEarly/432/`





# Pit

Monitor Example:

(1) Don't restart on each test

```xml
...

<Agent name="LocalAgent">
  <Monitor class="Process">
    <Param name="Executable" value="/path-to-under-test-program/" />
    <Param name="Arguments" value="...options..." />
    <Param name="RestartOnEachTest" value="false" />
    <Param name="FaultOnEarlyExit" value="true" />
  </Monitor>
</Agent>

<Test name="Default">
  <Agent ref="LocalAgent" />
  ...
</Test>
```

(2) Restart on each test

```xml
...

<Agent name="LocalAgent">
  <Monitor class="Process">
    <Param name="Executable" value="/path-to-under-test-program/" />
    <Param name="Arguments" value="...options..." />
    <Param name="RestartOnEachTest" value="true" />
    <Param name="FaultOnEarlyExit" value="false" />
  </Monitor>
</Agent>

<Test name="Default">
  <Agent ref="LocalAgent" />
  ...
</Test>
```



