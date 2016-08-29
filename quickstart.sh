#!/usr/bin/env bash
# (c) 2016, Matthew Hartstonge <matthew.hartstonge@imgtec.com> 
#
# Copyright (C) 2016 Imagination Technologies
# All Rights Reserved. Proprietary and confidential.
# Unauthorized copying of this file, via any medium is strictly prohibited.
#
# Description:
#   A Kickstarting bash script for the linux (ubuntu) user. Will run either a
#   Docker Compose stack, or fire up a vagrant virtual machine via virtualbox
#   Install the needed applications and then runs the Docker Compose stack
#   within the Virtualbox virtual machine.
#
#   Install prerequisites:
#     docker-compose:
#       Host:
#         - Native Linux/Ubuntu
#         - Virtual Linux/Ubuntu
#       Applications:
#       - docker-engine==1.10+
#       - docker-compose==1.6.0+
#     vagrant:
#       Host:
#         - Native Linux/Ubuntu
#       Applications:
#       - virtualbox==5.0+
#       - vagrant==1.8+

# Constants
LINE="-------------------------------------------------------------------------------"
IS_NATIVE_DOCKER="False"
MINIMUM_DOCKER_ENGINE_MAJOR=1
MINIMUM_DOCKER_ENGINE_MINOR=10
MINIMUM_DOCKER_COMPOSE_MAJOR=1
MINIMUM_DOCKER_COMPOSE_MINOR=6

function limitNumberOfAttempts() {
    limit=$1
    attempts=$2
    if [ -z "$attempts" ]; then
        attempts=1;
    else
        attempts=$((attempts + 1));
    fi
    if [ $attempts -ge $limit ]; then
        echo "Too many tries, failing..";
        exit 1;
    fi
    return 0;
}

# Check prerequisites
# - Internet Connection
# - Vagrant
function checkInternetConnectionActive() {
    ping google.com -c 2 > /dev/null 2>&1;
    case "$?" in
        0) echo "[ OK ] Internet Connection found" ;;
        *) echo "[FAIL] Internet Connection not found"
           echo "           Please Ensure you have an available internet connection!"
           return 1;;
    esac
    return 0;
}

function checkVirtualBoxInstalled() {
    virtualbox version > /dev/null 2>&1
    case "$?" in
        0) echo "[ OK ] Virtualbox installed" ;;
        *) echo "[FAIL] Virtualbox check has failed..."
           echo "           Please make sure Virtualbox is installed!"
           return 1;;
    esac
    return 0;
}

function checkVagrantInstalled() {
    vagrant version > /dev/null 2>&1
    case "$?" in
        0) echo "[ OK ] Vagrant installed" ;;
        *) echo "[FAIL] Vagrant check has failed..."
           echo "           Please make sure Vagrant is installed!"
           return 1 ;;
    esac
    return 0;
}

function checkDockerEngineVersion() {
    IFS='.' read -ra docker_engine_version <<< $(docker version -f '{{ .Server.Version }}')
    docker_engine_major="${docker_engine_version[0]}";
    docker_engine_minor="${docker_engine_version[1]}";
    docker_engine_patch="${docker_engine_version[2]}";
    if (( $docker_engine_minor >= $MINIMUM_DOCKER_ENGINE_MINOR )); then
        echo "[ OK ] Docker-engine version greater or equal to 1.10!"
    elif (( $docker_engine_minor < $MINIMUM_DOCKER_ENGINE_MINOR )); then
        echo "[FAIL] Docker-engine version lower than 1.10!"
        return 1
    fi
    return 0
}

function checkDockerComposeVersion() {
    IFS='.' read -ra docker_compose_version <<< $(docker-compose version --short)
    docker_compose_major="${docker_compose_version[0]}"
    docker_compose_minor="${docker_compose_version[1]}"
    docker_compose_patch="${docker_compose_version[2]}"
    if (( $docker_compose_minor >= $MINIMUM_DOCKER_COMPOSE_MINOR )); then
        echo "[ OK ] Docker-compose version greater than 1.6.0!"
    elif (( $docker_compose_minor < $MINIMUM_DOCKER_COMPOSE_MINOR )); then
        echo "[FAIL] Docker-compose version lower than 1.6.0!"
        return 1
    fi
    return 0;
}

function performNativeDockerPrerequisiteChecks() {
    checkDockerEngineVersion && \
    checkDockerComposeVersion
}

function performVagrantPrerequisiteChecks() {
    checkInternetConnectionActive && \
    checkVirtualBoxInstalled && \
    checkVagrantInstalled
}

function performPrerequisiteChecks() {
    echo
    echo $LINE
    echo "Performing Prerequisite Checks"
    echo $LINE

    if [ "${IS_NATIVE_DOCKER}" == "False" ]; then
        performVagrantPrerequisiteChecks
    else
        performNativeDockerPrerequisiteChecks
    fi

    case "$?" in
        0) echo "[ OK ] All prerequisite Checks passed!" ;;
        *) echo "[FAIL] Prerequisite Checks Failed!"
           echo "exiting..."
           exit 1;;
    esac

    echo $LINE
    echo
    return 0;
}


function decideOnDomainName() {
    attempts=$1
    read -r -p "Enter a FQDN hostname for the Device Server: " domainName
    if [ -z "$domainName" ]; then
        limitNumberOfAttempts 3 ${attempts}
        decideOnDomainName ${attempts}
    fi
}

function generateEnvFile() {
    echo
    echo $LINE
    echo "Setup an environment file"
    echo $LINE

    decideOnDomainName

    echo "# Host Environment variables for docker-compose services" > .env
    echo "export DEVICESERVER_HOSTNAME=${domainName}" >> .env
    # Import ENVar for the script
    source .env
    grep DEVICESERVER_HOSTNAME /etc/profile.d/deviceserver.sh  > /dev/null 2>&1
    if [ $? -ne 0 ]; then
        sudo mv /etc/profile.d/deviceserver.sh .
        sudo cat .env >> deviceserver.sh
        sudo mv deviceserver.sh /etc/profile.d/
    fi

    echo "[ OK ] Environment file created!"
    echo $LINE
}

function generateTlsCertificates() {
    echo
    echo $LINE
    echo "Setup Self Signed TLS Certificates"
    echo $LINE
    # Take in vars for CSR request
    read -r -p "Country Name (2 letter code) [AU]: " countryName
    read -r -p "State or Province Name (full name) [Some-State]: " provinceName
    read -r -p "Locality Name (eg, city) []: " localityName
    read -r -p "Organization Name (eg, company) [Internet Widgits Pty Ltd]: " organisationName
    read -r -p "Organizational Unit Name (eg, section) []: " organisationalUnitName
    read -r -p "Common Name (e.g. domain name/server FQDN) []: " commonName

    # Generate a Private Key and a CSR
    openssl req \
           -newkey rsa:2048 -nodes -keyout docker/ssl/domain_key.pem \
           -subj "/C=$countryName/ST=$provinceName/L=$localityName/O=$organisationName/OU=$organisationalUnitName/CN=$commonName" \
           -out docker/ssl/domain_csr.pem

    # Generate a Self-Signed Certificate from an Existing Private Key and CSR
    openssl x509 \
           -signkey docker/ssl/domain_key.pem \
           -in docker/ssl/domain_csr.pem \
           -req -days 365 -out docker/ssl/domain_cert.pem

    # Generate Diffie-Hellman certificate
    sudo openssl dhparam -out docker/ssl/dhparam.pem 2048

    # rename certificates for fabio
    cp -p docker/ssl/domain_cert.pem docker/ssl/cert.pem
    cp -p docker/ssl/domain_key.pem docker/ssl/key.pem

    echo "[ OK ] TLS Certificates created!"
    echo $LINE
}

function runDockerComposeUp() {
    echo
    echo $LINE
    echo "Setup Docker host"
    echo $LINE
    docker-compose up -d --force-recreate

    echo
    case "$?" in
        0) echo "[ OK ] docker-compose up succeeded!" ;;
        *) echo "[FAIL] docker-compose up Failed!"
           echo "exiting..."
           exit 1;;
    esac

    echo "The device server should now be accessible on http://localhost/"
    echo $LINE
}

function runVagrantUp() {
    echo
    echo $LINE
    echo "Setup Vagrant host"
    echo $LINE
    vagrant up

    echo
    case "$?" in
        0) echo "[ OK ] Vagrant up succeeded!" ;;
        *) echo "[FAIL] Vagrant up Failed!"
           echo "exiting..."
           exit 1;;
    esac

    echo "The device server should now be accessible on http://10.0.0.100/"
    echo $LINE
}

function runDeviceServerStack() {
    if [ "${IS_NATIVE_DOCKER}" = "True" ]; then
        runDockerComposeUp;
    else
        runVagrantUp;
    fi
}

function decideOnDockerOrVagrant() {
    attempts=$1
    read -r -p "Would you prefer native docker over Vagrant? [y/n]: " decision
    if [ -z "${decision}" ] && [ "${decision}" != "y" ] && [ "${decision}" != "Y" ] && [ "${decision}" != "n" ] && [ "${decision}" != "N" ]; then
        limitNumberOfAttempts 3 ${attempts}
        decideOnDockerOrVagrant ${attempts}
    fi
}

function areYouUsingDockerOrVagrant() {
    if [ -z "${attempts}" ]; then
        echo $LINE
        echo "Docker or Vagrant?"
        echo $LINE
    fi
    decideOnDockerOrVagrant
    if [ "${decision}" = "Y" ] || [ "${decision}" = "y" ]; then
        IS_NATIVE_DOCKER="True"
        echo "[ OK ] Using native docker"
    else
        IS_NATIVE_DOCKER="False"
        echo "[ OK ] Using Vagrant"
    fi
}

function main() {
    if [ "$1" = "dockermode" ]; then
        IS_NATIVE_DOCKER="True"
        echo "[ OK ] Using native docker"
    else
        areYouUsingDockerOrVagrant
    fi        
    performPrerequisiteChecks
    generateEnvFile
    generateTlsCertificates
    runDeviceServerStack
}

main $1
