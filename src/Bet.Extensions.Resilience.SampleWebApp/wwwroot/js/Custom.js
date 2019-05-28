
$.getJSON('https://localhost:5001/api/songs?count=100', function (data) {

    var list = [];
    data.forEach(function (item) {
        list.push({ 'icon': item.albumArtUri, 'title': item.name, 'file': item.uri });
    });

    AP.init(
        {
            container: '#player',//a string containing one CSS selector
            volume: 0.7,
            autoPlay: true,
            notification: true,
            playList: list
        });
});



