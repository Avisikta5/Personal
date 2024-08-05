function getSuggestion() {
    let timeOut;
    clearTimeout(timeOut);
    timeOut = setTimeout(() => {
        let input = document.querySelector(".nav-search-input").value;
        if (input.length > 0) {
            $.ajax({
                url: "/Product/AutoComplete",
                type: "GET",
                data: { input: input },
                success: function (data) {
                    console.log(data)
                    depictSuggestion(data);

                },
                error: function (error) {
                    console.log(error);
                }
            })
        }
        else {
            const suggestionContainer = document.querySelector(".search-suggestion-container");
            suggestionContainer.style.visibility = "hidden";
            suggestionContainer.style.opacity = 0;
        }
    }, 500);
}

function depictSuggestion(data) {
    const suggestionContainer = document.querySelector(".search-suggestion-container");
    suggestionContainer.style.visibility = "visible";
    suggestionContainer.style.opacity = 1;

    const suggestion = document.querySelector(".suggestion");
    suggestion.innerHTML = "";

    if (data.length === 0) {
        suggestion.innerHTML += "<li>No match found</li>";
    }
    else {
        let searchInnerString = ''
        data.map((item) => {
            searchInnerString += `<li onClick="setInputValue('${item}')">${item}</li>`;
        });
        suggestion.innerHTML = searchInnerString;
    }
}

function setInputValue(item) {
    let searchInput = document.querySelector(".nav-search-input");
    searchInput.value = item;
}