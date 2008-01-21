if ENABLE_MONODOC
SOURCESDIR=$(prefix)/lib/monodoc/sources
TARGETS=ndesk-options-docs.zip ndesk-options-docs.tree ndesk-options-docs.source
else
SOURCESDIR=
TARGETS=
endif

monodocdir=$(SOURCESDIR)
monodoc_DATA=$(TARGETS)

ASSEMBLIES = \
	net2/NDesk.Options.dll \
	net3.5/NDesk.Options.dll

UPDATER = $(MONODOCER) -overrides -pretty -path:en

assemble: ndesk-options-docs.zip ndesk-options-docs.tree

$(srcdir)/en/examples/NDesk.Options.dll: ../lib/ndesk-options/net2/NDesk.Options.dll
	cp $^ $@

%.exe: %.cs $(srcdir)/en/examples/NDesk.Options.dll
	gmcs -r:Mono.Posix.dll -lib:en/examples -r:NDesk.Options.dll -out:$@ $<

$(srcdir)/en/examples/locale/es/LC_MESSAGES/localization.mo: $(srcdir)/en/examples/localization-es.po
	msgfmt $< -o $@

$(srcdir)/en/examples/localization.exe: $(srcdir)/en/examples/locale/es/LC_MESSAGES/localization.mo

EXAMPLES_OUTPUT = \
	$(srcdir)/en/examples/context.txt \
	$(srcdir)/en/examples/greet.txt \
	$(srcdir)/en/examples/localization.txt

%.txt: %.in %.exe
	-rm $@
	exec 3<$< ; \
	while read line 0<&3 ; do \
		echo "$$ $$line" | sed 's#en/examples/##' >> $@ ; \
		script -q -a -c "$$line" $@ ; \
	done ; \
	exec 3>&-;

ndesk-options-docs.zip ndesk-options-docs.tree: $(srcdir)/en/*/*.xml $(srcdir)/en/*.xml
	$(MDASSEMBLER) --ecma $(srcdir)/en -o ndesk-options-docs

update: $(addprefix ../lib/ndesk-options/, $(ASSEMBLIES)) $(srcdir)/en/examples/*.cs $(EXAMPLES_OUTPUT)
	for a in $(ASSEMBLIES) ; do \
		$(UPDATER) -assembly:../lib/ndesk-options/$$a ; \
	done

update-delete: $(addprefix ../lib/ndesk-options/, $(ASSEMBLIES)) $(srcdir)/en/examples/*.cs $(EXAMPLES_OUTPUT)
	for a in $(ASSEMBLIES) ; do \
		$(UPDATER) --delete -assembly:../lib/ndesk-options/$$a ; \
	done

CLEANFILES = \
	ndesk-options-docs.zip \
	ndesk-options-docs.tree \
	$(srcdir)/en/examples/NDesk.Options.dll \
	$(srcdir)/en/examples/*.exe

edit:
	monodoc --edit $(srcdir)/en

clean:
	-rm $(CLEANFILES)
