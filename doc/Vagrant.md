# Vagrant DeviceServer

The Vagrantfile provided fires up and provisions a VM that can be based on your
 favorite operating system (at the moment CentOS 7 and Ubuntu). Under the hood 
 Ansible is used to configure docker-engine and docker-compose. Once everything
 is in place it runs docker-compose up

# Create the VM
After having [vagrant][vg] and [VirtualBox][vb] installed, enter the repository root and
run

```sh
vagrant up
```

This should install everything and provision on the first run. If errors occur
 first try reprovisioning, then if all else fails, destroy and run up again:

```sh
vagrant provision
# If it hits the fan
vagrant destroy && vagrant up
```

# Access services
The image sets up the VM to have endpoints and services accessible from:

```sh
10.0.0.100
```
 
# SSH Access
By ssh'ing in you can mangle anything within the VM, run docker commands, 
and also install your preferred applications.

```sh
vagrant ssh
```

The provisioning step adds the native OS user to the docker group to run docker
commands, or alternatively prepend all docker commands with sudo.

[vg]: <https://www.vagrantup.com/downloads.html>
[vb]: <https://www.virtualbox.org/wiki/Downloads>