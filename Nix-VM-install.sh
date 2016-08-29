#!/usr/bin/env bash
# This script installs all required to get a DeviceServer up and running on a
# Linux server.  It is aimed to work on
#   - CentOS v7
#   - Ubuntu v16.04
#    
# This has been tested using publicly available OpenStack images of the above
# two Linux versions.
#
# Contants
MINKERNELVERSION="3.10.0"


function waitforapt(){
    sudo fuser /var/lib/dpkg/lock
    ps waux | grep -i unattended
    CNT=0
    MAXCNT=600
    while [ "$CNT" -lt "$MAXCNT" ]; do
        if sudo fuser /var/lib/dpkg/lock >/dev/null 2>&1; then
            sleep 1
            let "CNT+=1"
            if [ $(( $CNT % 8 )) -eq 0 ]; then
                echo ">>>>> waiting for apt: $CNT"
            fi
        else
            echo ">>>>> waiting for apt - done: $CNT"
            CNT=1000
        fi
    done
}

function ubuntuplatform(){
    # install required packages
    sudo apt-get update 
    sudo apt-get -y install \
        python \
        python-dev \
        python-crypto \
        python-cffi \
        python-jinja2 \
        python-markupsafe \
        python-paramiko \
        python-setuptools \
        python-ecdsa \
        python-pip
    sudo -H pip install --upgrade pip

    # install docker, docker-compose and associated tools
    sudo apt-get -y install docker.io
    sudo -H pip install docker-compose==1.7.1
    sudo systemctl start docker.service
    sudo usermod -aG docker ubuntu
    sudo systemctl enable docker.service
}

function centosplatform(){
    # install epel to be able to install python-pip
    sudo yum -y install epel-release

    # install required packages
    sudo yum install -y \
        python \
        python-devel \
        python2-crypto \
        python-cffi \
        python-jinja2 \
        python-markupsafe \
        python2-paramiko \
        python-setuptools \
        python2-ecdsa \
        python-pip

    # install docker repo to be able to install docker, docker-compose 
    # and associated tools
    sudo tee /etc/yum.repos.d/docker.repo <<-'EOF'
[dockerrepo]
name=Docker Repository
baseurl=https://yum.dockerproject.org/repo/main/centos/7/
enabled=1
gpgcheck=1
gpgkey=https://yum.dockerproject.org/gpg
EOF

    sudo yum -y install docker-engine
    sudo systemctl start docker.service
    sudo groupadd docker
    sudo usermod -aG docker centos
    sudo systemctl enable docker.service
    curl -L https://github.com/docker/compose/releases/download/1.7.1/docker-compose-`uname -s`-`uname -m` > docker-compose
    sudo mv docker-compose /usr/local/bin/
    sudo chmod +x /usr/local/bin/docker-compose
    sudo echo "PATH=/usr/local/bin:$PATH" > deviceserver.sh
    sudo echo "export PATH" >> deviceserver.sh
    sudo mv deviceserver.sh /etc/profile.d/
}

function VERSION_NUM(){
    declare -a TEMP2
    TEMP2=(${1//[.-]/ })
    VERSIONNUM=$(((${TEMP2[0]} * 10000) + (${TEMP2[1]} * 100) + ${TEMP2[2]}))
}

function main(){
    # Check Kernel version
    VERSION_NUM $MINKERNELVERSION
    MINVERSIONNUM=$VERSIONNUM

    KERNELVERSION=$(uname --kernel-release)
    VERSION_NUM $KERNELVERSION

    if [ "$VERSIONNUM" -lt "$MINVERSIONNUM" ]; then
        echo "Invalid kernel version!"
        echo "Should be less than $MINKERNELVERSION, but is $KERNELVERSION."
        exit
    fi


    # Get platform
    if [ -f /etc/os-release ]; then
        PLATFORM=`awk -F= '/^ID=/{print tolower($2)}'  /etc/os-release | tr -d '"'`
        RELEASE=`awk -F= '/VERSION_ID=/{print tolower($2)}'  /etc/os-release | tr -d '"'`
    elif [ -f /etc/lsb-release ]; then
        PLATFORM=`awk -F= '/ID=/{print tolower($2)}'  /etc/lsb-release | tr -d '"'`
        RELEASE=`awk -F= '/DISTRIB_RELEASE=/{print tolower($2)}'  /etc/lsb-release | tr -d '"'`
    elif [ -f /etc/centos-release ]; then
        PLATFORM=`awk '{print tolower($1)}'  /etc/centos-release | tr -d '"'`
        RELEASE=`awk '{print tolower($3)}'  /etc/centos-release | tr -d '"'`
    else
        echo "Cannot find release file !!"
    fi

    # Check platform and release
    # For now only CentOS v7 and Ubuntu 16.04 have been tested.
    fAbort="False"
    if [ "$PLATFORM" == "centos" ]; then
        if [ "$RELEASE" != "7" ]; then
            fAbort="True"
        fi
    elif [ "${PLATFORM}" == "ubuntu" ]; then
        if [ "$RELEASE" != "16.04" ]; then
            fAbort="True"
        fi
    else
        fAbort="True"
    fi

    if [ "$fAbort" = "True" ]; then
        echo "Aborting, untested environment!!"
        echo "Platform found: $PLATFORM"
        echo "Release  found: $RELEASE"
        exit
    fi    

    # Set hostname
    sudo -H sed -i "/127.0.0.1/ s/$/ $HOSTNAME/" /etc/hosts

    # Install packages
    if [ "${PLATFORM}" == "ubuntu" ] || [ "${PLATFORM}" == "debian" ]; then
        ubuntuplatform
    elif [ "${PLATFORM}" == "centos" ]; then
        centosplatform
    fi
}

main
