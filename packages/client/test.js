console.log('in tetsstst');

function _base64ToArrayBuffer(base64) {
  const binary_string = window.atob(base64);
  const len = binary_string.length;
  const bytes = new Uint8Array(len);
  for (var i = 0; i < len; i++) {
    bytes[i] = binary_string.charCodeAt(i);
  }
  // const blob = new Blob([bytes]);
  return bytes;

  // return bytes.buffer;
  // return blob;
}

fetch('/api/final', {
  method: 'POST',
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
  // body: JSON.stringify({
  //   // settingsString: genSettingsString(),
  //   settingsString: settingsString,
  //   // uSettingsString: ,
  //   seed: $('#seed').val(),
  // }),
})
  .then((response) => response.json())
  .then((data) => {
    console.log(data);
    if (data.data && data.data.meta && data.data.bytes) {
      const { meta, bytes } = data.data;
      const allFileBytes = _base64ToArrayBuffer(bytes);

      let currIndex = 0;
      meta.forEach(({ name, length }) => {
        const fileBytes = allFileBytes.slice(currIndex, currIndex + length);

        const blob = new Blob([fileBytes]);

        const link = document.createElement('a');
        link.href = URL.createObjectURL(blob);
        link.download = name;
        link.innerHTML = 'Click here to download the file';
        document.body.appendChild(link);
        currIndex += length;
      });
    } else {
      console.error('PROBLEM');
    }
  })
  .catch((err) => {
    console.log('ERROR');
    console.log(err);
  });
