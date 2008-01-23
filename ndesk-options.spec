
Summary: NDesk.Options is a C# program option parsing library, inspired by Getopt::Long.
Name: ndesk-options
Version: 0.1.0
Release: 1
License: MIT
Group: Development/Languages/Mono
Requires: mono-core >= 1.2.0
Requires: monodoc-core >= 1.2.0
Source: http://www.jprl.com/Projects/ndesk-options/ndesk-options-0.1.0.tar.gz
URL: http://www.jprl.com/Projects/ndesk-options
Packager: Jonathan Pryor <jonpryor@vt.edu>
BuildArch: noarch
BuildRoot: %{_tmppath}/%{name}-root

%description
NDesk.Options is a C# program option parsing library, 
inspired by Getopt::Long.

%prep
%setup

%build
./configure --prefix=/usr --sysconfdir=/etc --localstatedir=/var
make

%install
make DESTDIR=%{buildroot} install

%clean
rm -rf %{buildroot}
make clean

%files
/usr/lib

%changelog

