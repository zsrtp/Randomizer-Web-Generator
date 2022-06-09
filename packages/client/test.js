console.log('in tetsstst');

function _base64ToUint8Array(base64Str) {
  const binary_string = window.atob(base64Str);
  const len = binary_string.length;
  const bytes = new Uint8Array(len);
  for (var i = 0; i < len; i++) {
    bytes[i] = binary_string.charCodeAt(i);
  }
  return bytes;
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
  .then(({ error, data }) => {
    if (error) {
      console.log('error in response');
      console.log(error);
    } else if (data) {
      data.forEach(({ name, bytes }) => {
        const fileBytes = _base64ToUint8Array(bytes);

        const link = document.createElement('a');
        link.href = URL.createObjectURL(new Blob([fileBytes]));
        link.download = name;
        link.innerHTML = 'Click here to download the file';
        document.body.appendChild(link);
      });
    }
  })
  .catch((err) => {
    console.log('ERROR');
    console.log(err);
  });
