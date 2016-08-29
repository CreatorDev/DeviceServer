# Ansible Tasks
Herein lie all the YAML defined tasks relating to automating a given role.

For example,

```yaml
---
- name: Add Nginx Repository
  apt_repository: repo='ppa:nginx/stable' state=present

- name: Install Nginx
  apt: pgk=nginx state=latest update_cache=true
  notify:
   - Start Nginx

- name: Add H5BP Config
  copy: src=h5bp dest=/etc/nginx owner=root group=root

- name: Disable Default Config
  file: dest=/etc/nginx/sites-enabled/defalt state=absent
  notify:
    - Reload Nginx

- name: Add SFH Site Config
  template: src=serversforhackers.com.j2 dest=/etc/nginx/sites-available/{{ domain }} owner=root group=root

- name: Enable SFH Site Config
  file: src=/etc/nginx/sites-available/{{ domain }} dest=/etc/nginx/sites-enabled/{{ domain }} state=link
  notify:
    - Reload Nginx
```