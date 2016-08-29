# Ansible handlers

Handlers are any tasks you want to run after completing a task. For example, enabling, starting or stopping a sevice.
Handlers have no guarantee of completing successfully.

```yaml
---
- name: Start MySQL
  service:
    name: mysql
    state: started

- name: Restart MySQL
  service:
    name: mysql
    state: restarted

- name: Stop MySQL
  service:
    name: mysql
    state: stopped
```