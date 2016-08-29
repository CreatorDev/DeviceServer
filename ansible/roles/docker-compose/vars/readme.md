# Ansible Variables

As mentioned in templates, the Ansible crew frown on merging dictionaries, so it is advisable to keep all variables separate but prefixed with a namespace to help avoid issues.

A namespaced example

```yaml
wordpress_domain: 'example.com'
wordpress_database_prefix: 'wp_db_prefix_'
wordpress_salt: 'kansc-09wu1enl;s-12h11231we1cz7cains'
wordpress_user: 'jimmybobAdmin'
```