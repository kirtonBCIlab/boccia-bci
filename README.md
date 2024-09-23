# Boccia BCI


## Python development

Use a conda environment to set up development tooling:
```
cd <repo>/Boccia-Python/
conda env create -f ./environment.yml
conda activate boccia
```

Install dependencies with pip, use -e for development (local changes picked up without reinstalling)
```
pip install -e .
```

Use headset sim script if a dummy eeg is needed:
```
python ./headset_sim.py
```

To start back end:
```
<start streaming eeg over LSL>
python ./main.py
```

If backend won't exit properly, kill the job
```
(ctrl + Z)
kill %1
```

This is a test
