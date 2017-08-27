(New-Object Net.WebClient).DownloadFile('http://download.geteventstore.com/binaries/EventStore-OSS-Win-v4.0.2.zip','c:\tools\eventstore.zip')
7z e C:\tools\eventstore.zip -oC:\tools\EventStore
start-process -NoNewWindow "C:\tools\EventStore\EventStore.ClusterNode.exe" "--memdb --run-projections ALL"