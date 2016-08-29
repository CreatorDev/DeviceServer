#!/usr/bin/env bash

function fixUbuntuNetworking(){
    releaseVersion=$(python -c "import platform;print(platform.linux_distribution()[1])")
    if [ "$releaseVersion" == "16.04" ]; then
    # Fix hostname not resolving
    ADDED=$(`grep "127.0.1.1 ubuntu-xenial" /etc/hosts >> /dev/null` && echo $? || echo 1)
    if [ ${ADDED} == 1 ]; then
        sudo echo "127.0.1.1 ubuntu-xenial" >> /etc/hosts
    fi
    echo "
        auto enp0s8
        iface enp0s8 inet manual
        pre-up sleep 2" > /etc/network/interfaces.d/enp0s8.cfg; ifup enp0s8;ifconfig enp0s8 10.0.0.100 netmask 255.255.255.0
    fi
}

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

function debian(){
    # Run the hax for Vagrant & 16.04 networking failing
    fixUbuntuNetworking
    waitforapt
    sudo apt-get update 
    waitforapt
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
}

function enterpriseLinux(){
    yum install -y \
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
}

function installAnsible(){
    pip install ansible==2.1.0.0
}

function installApps(){
    cd /mnt/DeviceServer/ansible/
    ansible-playbook playbook.yml -i inventory/hosts
}

function startStack(){
    # Ensure mongo has an ENV variable set so it can store it's data when
    # docker-compose up is run
    ADDED=$(`grep 'PERSISTENT_DATA_DIR=""' ~/.bashrc >> /dev/null` && echo $? || echo 1)
    if [ ${ADDED} == 1 ]; then
        sudo echo 'PERSISTENT_DATA_DIR=""' >> ~/.bashrc
    fi
    source ~/.bashrc
    mkdir -p /mongo

    cd /mnt/DeviceServer && docker-compose pull && docker-compose up --force-recreate -d
}

function main(){
    # just wait for unattended updates to start, to prevent race conditions
    sleep 10

    # Get platform
    if [ -f /etc/os-release ]; then
        PLATFORM=`awk -F= '/^ID=/{print tolower($2)}'  /etc/os-release | tr -d '"'`
    elif [ -f /etc/lsb-release ]; then
        PLATFORM=`awk -F= '/ID=/{print tolower($2)}'  /etc/lsb-release | tr -d '"'`
    elif [ -f /etc/centos-release ]; then
        PLATFORM=`awk '{print tolower($1)}'  /etc/centos-release | tr -d '"'`
    else
        echo "Cannot find release file !!"
    fi

    # Install packages
    if [ "${PLATFORM}" == "ubuntu" ] || [ "${PLATFORM}" == "debian" ]; then
        debian
    elif [ "${PLATFORM}" == "centos" ]; then
        enterpriseLinux
    fi
    installAnsible
    # Install apps as per Ansible
    installApps
    startStack
}

main
