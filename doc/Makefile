ASSEMBLIES = \
	net2/NDesk.Options.dll \
	net3.5/NDesk.Options.dll

MONODOCER = monodocer
MDASSEMBLER = mdassembler

UPDATER = $(MONODOCER) -overrides -pretty -path:en

SOURCESDIR=$(prefix)/lib/monodoc/sources
TARGETS=ndesk-options-docs.zip ndesk-options-docs.tree ndesk-options-docs.source

assemble: mono-fuse-docs.zip mono-fuse-docs.tree

mono-fuse-docs.zip mono-fuse-docs.tree: en/*/*.xml en/*.xml
	$(MDASSEMBLER) --ecma en -o ndesk-options-docs

update: $(addprefix ../lib/ndesk-options/, $(ASSEMBLIES))
	for a in $(ASSEMBLIES) ; do \
		$(UPDATER) -assembly:../lib/ndesk-options/$$a ; \
	done

update-delete: $(addprefix ../lib/ndesk-options/, $(ASSEMBLIES))
	for a in $(ASSEMBLIES) ; do \
		$(UPDATER) --delete -assembly:../lib/ndesk-options/$$a ; \
	done

CLEANFILES = mono-fuse-docs.zip mono-fuse-docs.tree

edit:
	monodoc --edit $(srcdir)/en

