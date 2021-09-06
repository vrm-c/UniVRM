import sys
import time
import subprocess
from watchdog.observers import Observer
from watchdog.events import PatternMatchingEventHandler
import pathlib

HERE = pathlib.Path(__file__).absolute().parent


class MyHandler(PatternMatchingEventHandler):
    def __init__(self):
        super(MyHandler, self).__init__(patterns='*.yml;*.md')

    def _run_command(self):
        subprocess.call(['docfx.exe', 'build'])

    def on_moved(self, event):
        self._run_command()

    def on_created(self, event):
        self._run_command()

    def on_deleted(self, event):
        self._run_command()

    def on_modified(self, event):
        self._run_command()


def watch():
    event_handler = MyHandler()
    observer = Observer()
    observer.schedule(event_handler, HERE, recursive=True)
    observer.start()
    try:
        while True:
            time.sleep(1)
    except KeyboardInterrupt:
        observer.stop()
    observer.join()


if __name__ == "__main__":
    watch()
