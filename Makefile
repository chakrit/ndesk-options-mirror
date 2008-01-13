
OPTIONS_SRC = src/Mono.Program/Mono.Program/Options.cs

bindir=bin
libdir=lib/mono-program

CSC = gmcs

all: $(libdir)/Mono.Program.net2.dll \
		$(libdir)/Mono.Program.net3.dll
	
$(libdir)/Mono.Program.net2.dll:
	$(CSC) -debug+ $(OPTIONS_SRC) -t:library -out:$@
	
$(libdir)/Mono.Program.net3.dll:
	$(CSC) -debug+ -d:LINQ -r:System.Core.dll $(OPTIONS_SRC) -t:library -out:$@

$(bindir)/options-test2.exe:
	$(CSC) -debug+ -d:TEST $(OPTIONS_SRC) -t:exe -out:$@

$(bindir)/options-test3.exe:
	$(CSC) -debug+ -d:LINQ -r:System.Core.dll -d:TEST $(OPTIONS_SRC) -t:exe -out:$@

check: $(bindir)/options-test2.exe $(bindir)/options-test3.exe
	mono --debug bin/options-test2.exe && mono --debug bin/options-test3.exe

clean:
	-rm \
		$(bindir)/options-test2.exe           \
		$(bindir)/options-test2.exe.mdb       \
		$(bindir)/options-test3.exe           \
		$(bindir)/options-test3.exe.mdb       \
		$(libdir)/Mono.Program.net2.dll       \
		$(libdir)/Mono.Program.net2.dll.mdb   \
		$(libdir)/Mono.Program.net3.dll	      \
		$(libdir)/Mono.Program.net3.dll.mdb   \

