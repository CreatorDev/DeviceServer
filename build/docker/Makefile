ORGANISATION?=creatordev
PREFIX?=${ORGANISATION}

IMAGE_VERSION?=latest

IMAGES= \
	ds-mono-webservice-deviceserver \
	ds-mono-lwm2m-bootstrap \
	ds-mono-lwm2m-server \
	ds-mono-service-changenotification \
	ds-mono-service-subscription \
	ds-mono-service-webhook

.PHONY: all base build tag_latest release

all: build

base:
	$(MAKE) -C alpine-mono

define BUILD_IMAGE

.PHONY: build-$(1) clean-$(1) tag_latest-$(1) release-$(1)

build: build-$(1)
build-$(1):
	cd ..; docker build -f docker/$(1)/Dockerfile -t $${PREFIX}/$(1):$${IMAGE_VERSION} --rm .

clean: clean-$(1)
clean-$(1):
	docker rmi $${PREFIX}/$(1):$${IMAGE_VERSION} || echo "Not found"

tag_latest: tag_latest-$(1)
tag_latest-$(1):
	@if [ "$${IMAGE_VERSION}" = "latest" ]; then echo '\nPlease set IMAGE_VERSION\n' && false; fi
	docker tag $${PREFIX}/$(1):$${IMAGE_VERSION} $${PREFIX}/$(1):latest )

release: release-$(1)
release-$(1):
	docker push $${PREFIX}/$(1):$${IMAGE_VERSION} && docker push $${PREFIX}/$(1):latest

endef

$(foreach image,$(IMAGES),$(eval $(call BUILD_IMAGE,$(image))))

