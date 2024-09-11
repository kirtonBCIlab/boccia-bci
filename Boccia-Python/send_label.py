import sys
import time

from bci_essentials.io.lsl_messenger import LslMessenger
from bci_essentials.classification.generic_classifier import Prediction


if len(sys.argv) < 3:
    print("\nSend predicted labels to Unity.\n\nUsage:\n" + sys.argv[0] + ": period label [label] ...")
    print("\nperiod = sleep between label messages in seconds")
    print("\label [label] ... = numeric labels to send, ex: 1 2 1 2 2\n")
    sys.exit()

# all arguments but first one is considered the list of labels
period = int(sys.argv[1])
labels = sys.argv[2:]

# send the labels out one at a time
messenger = LslMessenger()
for label in labels:
    print("sending predicted label: " + label)
    p = Prediction([label], [[]])
    messenger.prediction(p)
    time.sleep(period)
