console.log('in testtt');

fetch('/api/example', {
  method: 'POST',
  headers: {
    Accept: 'application/json',
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    settingsString: '0sPN11700000_91A8V_Jm-Gaq_u',
  }),
})
  .then((response) => response.json())
  .then((res) => {
    console.log('RESULT:');
    console.log(res);
  })
  .catch((err) => console.log(err));
