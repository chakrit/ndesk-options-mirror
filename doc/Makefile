ASSEMBLIES = \
	net2/NDesk.Options.dll \
	net3.5/NDesk.Options.dll

MONODOCER = monodocer
MDASSEMBLER = mdassembler

UPDATER = $(MONODOCER) -overrides -pretty -path:en

SOURCESDIR=$(prefix)/lib/monodoc/sources
TARGETS=ndesk-options-docs.zip ndesk-options-docs.tree ndesk-options-docs.source

assemble: mono-fuse-docs.zip mono-fuse-docs.tree

en/examples/NDesk.Options.dll: ../lib/ndesk-options/net2/NDesk.Options.dll
	cp $^ $@

%.exe: %.cs en/examples/NDesk.Options.dll
	gmcs -r:Mono.Posix.dll -lib:en/examples -r:NDesk.Options.dll -out:$@ $<

EXAMPLES_OUTPUT = \
	en/examples/localization.txt

%.txt: %.in %.exe
	-rm $@
	exec 3<$< ; \
	while read line 0<&3 ; do \
		echo "$$ $$line" | sed 's#en/examples/##' >> $@ ; \
		script -q -a -c "$$line" $@ ; \
	done ; \
	exec 3>&-;

mono-fuse-docs.zip mono-fuse-docs.tree: en/*/*.xml en/*.xml
	$(MDASSEMBLER) --ecma en -o ndesk-options-docs

update: $(addprefix ../lib/ndesk-options/, $(ASSEMBLIES)) en/examples/*.cs $(EXAMPLES_OUTPUT)
	for a in $(ASSEMBLIES) ; do \
		$(UPDATER) -assembly:../lib/ndesk-options/$$a ; \
	done

update-delete: $(addprefix ../lib/ndesk-options/, $(ASSEMBLIES)) en/examples/*.cs $(EXAMPLES_OUTPUT)
	for a in $(ASSEMBLIES) ; do \
		$(UPDATER) --delete -assembly:../lib/ndesk-options/$$a ; \
	done

CLEANFILES = \
	ndesk-options-docs.zip \
	ndesk-options-docs.tree \
	en/examples/NDesk.Options.dll \
	en/examples/*.exe \
	$(EXAMPLES_OUTPUT)

edit:
	monodoc --edit $(srcdir)/en

clean:
	-rm $(CLEANFILES)
