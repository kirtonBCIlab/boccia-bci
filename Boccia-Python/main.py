from bci_essentials.io.lsl_sources import LslEegSource, LslMarkerSource
from bci_essentials.io.lsl_messenger import LslMessenger
from bci_essentials.bci_controller import BciController
from bci_essentials.paradigm.p300_paradigm import P300Paradigm
from bci_essentials.data_tank.data_tank import DataTank
from bci_essentials.classification.erp_rg_classifier import ErpRgClassifier

# create LSL sources, these will block until the outlets are present
eeg_source = LslEegSource()
marker_source = LslMarkerSource()
messenger = LslMessenger()
paradigm = P300Paradigm()
data_tank = DataTank()

# Set classifier settings ()
classifier = ErpRgClassifier()  # you can add a subset here

# Set some settings
classifier.set_p300_clf_settings(
    n_splits=5,
    lico_expansion_factor=1,
    oversample_ratio=0,
    undersample_ratio=0,
    covariance_estimator="oas",
)

# Initialize the ERP
test_erp = BciController(
    classifier, eeg_source, marker_source, paradigm, data_tank, messenger
)

# Run main
test_erp.setup(
    online=True,
)
test_erp.run()







eg_source = EmotivEegSource()
marker_source = LslMarkerSource()
messenger = LslMessenger()

# Select a classifier
logger.debug("Selecting MiClassifier()")
classifier = MiClassifier()  # you can add a subset here

# Set settings
logger.debug("Setting classifier settigs")
classifier.set_mi_classifier_settings(n_splits=3, type="TS", random_seed=35)

# Define the MI data object
logger.debug("Defining the MI data object")
mi_data = EegData(classifier, eeg_source, marker_source, messenger)

# Run
logger.debug("Setting online to True and training to True")
mi_data.setup(online=True, training=True, live_update=True)
logger.debug("Starting run() method")
mi_data.run()
