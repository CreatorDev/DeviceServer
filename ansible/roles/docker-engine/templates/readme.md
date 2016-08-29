# Ansible Templates
Ansible templates use Python's Jinja2, but not all of it so there are a few caveats with templating, but minimal. In relation to variables, the Ansible crew frown on merging, so it is advisable to keep all variables separate but prefixed with a namespace to help avoid issues.

Example :

```yaml
- name: Configure Nginx
  template:
    src: /nginx.conf.j2
    dest: /etc/nginx/nginx.conf
    owner: root
    group: root
    mode: 0600
```
Template Variables under role/templates/main.yml

```yaml
domain: example.com
```

A very bad Nginx template. Make sure template names are ended with .j2 for conformity.
```jinja2
# Use h5bp Nginx config for security ;)
server {
    listen 80 default_server;
    server_name *.{{ '{{' }} domain {{ '}}'  }};

    root /var/www/{{ '{{' }} domain {{ '}}'  }}/public;
    index index.html index.htm index.php;

    location / {
        try_files $uri $uri/ /index.php$is_args$args;
    }
}
```
