ORGANISATION?=creatordev
PREFIX?=${ORGANISATION}

IMAGE_VERSION?=latest

IMAGES=\
	alpine-glibc \
	alpine-mono \
	ubuntu-mono \
	ds-mono-webservice-deviceserver \
	ds-mono-lwm2m-bootstrap \
	ds-mono-lwm2m-server \
	ds-mono-service-changenotification \
	ds-mono-service-subscription \
	ds-mono-service-webhook

UBUNTU_IMAGES=\
	ds-mono-webservice-deviceserver \
	ds-mono-lwm2m-bootstrap \
	ds-mono-lwm2m-server \
	ds-mono-service-changenotification \
	ds-mono-service-subscription \
	ds-mono-service-webhook

.PHONY: all build tag_latest release

all: build

define HEADER
	@echo
	@echo "*********************************************************************"
	@echo "*"
	@echo "*   $(1)"
	@echo "*"
	@echo "*********************************************************************"
	@echo
endef

define BUILD_IMAGE

.PHONY: build-$(1) clean-$(1) tag_latest-$(1) release-$(1)

build: build-$(1)
build-$(1):
	$(call HEADER,BUILD $(1))
	cd ..; docker build -f docker/$(1)/Dockerfile -t $${PREFIX}/$(1):$${IMAGE_VERSION} --rm .

clean: clean-$(1)
clean-$(1):
	$(call HEADER,CLEAN $(1))
	docker rmi $${PREFIX}/$(1):$${IMAGE_VERSION} || echo "Not found"

tag_latest: tag_latest-$(1)
tag_latest-$(1):
	$(call HEADER,TAG $(1))
	@if [ "$${IMAGE_VERSION}" = "latest" ]; then echo '\nPlease set IMAGE_VERSION\n' && false; fi
	docker tag $${PREFIX}/$(1):latest $${PREFIX}/$(1):$${IMAGE_VERSION}

release: release-$(1)
release-$(1):
	$(call HEADER,RELEASE $(1))
	docker push $${PREFIX}/$(1):$${IMAGE_VERSION} && docker push $${PREFIX}/$(1):latest

endef

define BUILD_UBUNTU
.PHONY: build-ubuntu-$(1) clean-ubuntu-$(1) tag_latest-ubuntu-$(1) release-ubuntu-$(1)

build: build-ubuntu-$(1)
build-ubuntu-$(1):
	$(call HEADER,BUILD $(1) (ubuntu))
	cat $(1)/Dockerfile | sed 's,alpine,ubuntu,g' > $(1)/Dockerfile.ubuntu
	cd ..; docker build -f docker/$(1)/Dockerfile.ubuntu -t $${PREFIX}/$(1):ubuntu-$${IMAGE_VERSION} --rm .

clean: clean-ubuntu-$(1)
clean-ubuntu-$(1):
	$(call HEADER,CLEAN $(1) (ubuntu))
	docker rmi $${PREFIX}/$(1):ubuntu-$${IMAGE_VERSION} || echo "Not found"

tag_latest: tag_latest-ubuntu-$(1)
tag_latest-ubuntu-$(1):
	$(call HEADER,TAG $(1) (ubuntu))
	@if [ "$${IMAGE_VERSION}" = "latest" ]; then echo '\nPlease set IMAGE_VERSION\n' && false; fi
	docker tag $${PREFIX}/$(1):ubuntu $${PREFIX}/$(1):ubuntu-$${IMAGE_VERSION}

release: release-ubuntu-$(1)
release-ubuntu-$(1):
	$(call HEADER,RELEASE $(1) (ubuntu))
	docker push $${PREFIX}/$(1):ubuntu-$${IMAGE_VERSION} && docker push $${PREFIX}/$(1):ubuntu

endef

$(foreach image,$(IMAGES),$(eval $(call BUILD_IMAGE,$(image))))
$(foreach image,$(UBUNTU_IMAGES),$(eval $(call BUILD_UBUNTU,$(image))))

