# Ansible Meta
Meta stores dependencies. As roles grow, you can start to depend on other roles. Dependencies always run before the main tasks.

```yaml
---
dependencies:
  - { role: openssh }
```

If there are no dependencies, just leave an empty list.

```yaml
---
dependencies: []
```