import re

class Helpers:
    MAXPRICE = 999999   
    @staticmethod
    def getNumber(pattern):
        """Convert price text to number. Ignores any decimals.
           >>> Helpers.getNumber("150 132 Ft")
           150132.0
           >>> Helpers.getNumber("150,132 Ft")
           150132.0
           >>> Helpers.getNumber("- Ft")
           0.0
           >>> Helpers.getNumber(None)
           0.0
        """
        if pattern:
            s = re.sub(r"[^\d]", "", pattern)
            if s == "":
                return 0.0
            else:
                return float(s)
        else:
            return 0.0

    @staticmethod
    def getLetters(pattern):
        """Strip pattern only to letters
           >>> Helpers.getLetters("150 132 Ft")
           'Ft'
           >>> Helpers.getLetters("150 132 Ft-")
           'Ft'
           >>> Helpers.getLetters("150&nbsp;132 Ft")
           'Ft'
           >>> Helpers.getLetters("150 132")
           ''
           >>> Helpers.getLetters(None)
           ''
        """
        if pattern:
            s = re.sub(r"[\d-]", "", pattern)
            s = s.replace("&nbsp;", " ")
            return s.strip()
        else:
            ""

    @staticmethod
    def getString(text):
        """Strip text
        """
        if text:
            s = text.replace("&nbsp;", " ")
            return s.strip()
        else:
            return ""

    # Keys should be in lower case
    currencies = {
        'ingyenes': 'HUF',
        'ft': 'HUF',
        'eur': 'EUR',
        'huf': 'HUF',
        '$': 'USD',
        'usd': 'USD',
        '€': 'EUR'
    }
    @staticmethod
    def getCurrency(pattern):
        """Get currency from pattern
           >>> Helpers.getCurrency("150 132 Ft")
           'HUF'
           >>> Helpers.getCurrency("price: 150 132 Ft")
           'HUF'
           >>> Helpers.getCurrency("price: 150 132")
           'HUF'
           >>> Helpers.getCurrency("150 EUR")
           'EUR'
           >>> Helpers.getCurrency("$150")
           'USD'
           >>> Helpers.getCurrency("None")
           'HUF'
        """
        if pattern:
            text = Helpers.getLetters(pattern).lower()
            for curr in Helpers.currencies:
                if text == curr:
                    return Helpers.currencies[curr]
            
            for curr in Helpers.currencies:
                if curr in text:
                    return Helpers.currencies[curr]
        return 'HUF'

    @staticmethod
    def imageUrl(response, imgurl):
        if imgurl:
            if response:
                return [response.urljoin(imgurl)]
            else:
                return [imgurl]
        else:
            return []

if __name__ == '__main__':
    import doctest
    doctest.testmod()