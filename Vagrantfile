# -*- mode: ruby -*-
# vi: set ft=ruby :

$script = <<SCRIPT
chmod +x ~/install.sh
~/install.sh
SCRIPT

# All Vagrant configuration is done below.
# Please don't change it unless you know what you're doing.
Vagrant.configure(2) do |config|
  # Provider Configuration
  config.vm.provider "virtualbox" do |vb|
    # Display the VirtualBox GUI when booting the machine
    vb.gui = false
    # Customize the amount of memory on the VM:
    vb.memory = "4096"
    vb.cpus = 4
  end

  # Enable automatic box update checking.
  config.vm.box_check_update = true

  # Choose a base VM
  # Ubuntu 12.04
  #config.vm.box = "ubuntu/precise64"
  # Ubuntu 14.04
  #config.vm.box = "ubuntu/trusty64"
  # Ubuntu 15.10
  #config.vm.box = "ubuntu/wily64"
  # Ubuntu 16.04
  config.vm.box = "bento/ubuntu-16.04"
  # config.vm.box = "ubuntu/xenial64"
  #config.vm.box = "bento/centos-6.7"
  #config.vm.box = "bento/centos-7.1"

  # Map device server folder
  config.vm.synced_folder "./", "/mnt/DeviceServer"

  # Configure Networking
  # A host-only private network gives access to the containers without port forwarding
  # The use of 'auto_config: false' is a workaround for:
  # mesg: ttyname failed: Inappropriate ioctl for device
  config.vm.network "private_network", ip: "10.0.0.100", auto_config: false

  # Provisioning Configuration
  config.vm.provision "shell", path: "ansible/install.sh"
end
