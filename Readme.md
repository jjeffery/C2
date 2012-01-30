## C2: Castle Contribution ##

This repository contains the project `C2.Facilities.NH`, which is a simple castle windsor facility for [NHibernate](http://nhforge.org) integration.

Note that there is already a facility in the [Castle repository](http://github.com/castleproject) for NHibernate integration, and it is officially supported, so check out that facility to see if it suits your needs better.

The official NHibernate integration facility has dependencies on the following assemblies:

* Castle.Core 2.5.1.0
* Castle.Services.Transactions 2.5.0.0
* Castle.Windsor 2.5.1.0
* NHibernate 3.1.0.4000

If you are able to stay with these older versions, then the official NHibernate integration facility might suit your purposes better that this one.

This facility has dependencies on the following assemblies:

* Castle.Core 3.0.0.4001
* Castle.Windsor 3.0.0.4001
* NHibernate 3.2.0.4000

## Why Write another NHibernate integration facility? ##

I wrote this facility because I needed to upgrade to the latest version of Castle.Windsor to satisfy other dependencies in a project I was working on. This then turned into an attempt to upgrade to the latest version of Castle.Services.Transactions. This assembly has undergone significant revision between 2.5 and 3.0, and the 3.0 version is compiled against version 2.5 of Castle.Core and Castle.Windsor. It all got a bit too hard for me, especially since none of the other code I was using made any use of Castle transactions.

I know it smacks of [NIH](http://en.wikipedia.org/wiki/Not_invented_here) syndrome, but I decided to write my own. At least it helped me to better understand how to write a castle windsor facility. I have made use of code from the official repository, as well as some good ideas from Henrik Feldt's [Castle.Facilities.NHibernate](https://github.com/haf/Castle.Facilities.NHibernate) facility. Thanks to Henrik and the Castle project contributors.


## About the Castle Project ##

This product incorporates source code from the Castle Project.

More information on the Castle Project can be found at [http://castleproject.org](http://castleproject.org).




