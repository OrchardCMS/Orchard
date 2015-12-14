#include <stdlib.h>
#include <windows.h>
#include "precomp.h"

#define MAX_VIEWSTATE               (1024)
#define VIEWSTATE                   "__VIEWSTATE"
#define VIEWSTATE_LEN               (10)
#define MAX_TAG_NAME                (128)

#define WCATRESULT_ERROR            (-1)
#define WCATRESULT_SUCCESS          (0)
#define WCATRESULT_MORE_DATA_NEEDED (1)
#define WCATRESULT_NEXT_PACKET      (2)

typedef enum _USER_STATE
{
    UserStateFindViewState,
    UserStateFindValue,
    UserStateFindEqual,
    UserStateFindFirstQuote,
    UserStateGetValue,
    UserStateFinished
} USER_STATE;

typedef struct _USER_DATA
{
    USER_STATE  state;
    char        viewstate[MAX_VIEWSTATE];
} USER_DATA, *PUSER_DATA;

BOOL WINAPI DllMain(
                IN HINSTANCE hinstDll,
                IN DWORD dwReason,
                IN LPVOID lpvContext
        )
{
    switch( dwReason ) {

    case DLL_PROCESS_ATTACH:
        break;

    case DLL_PROCESS_DETACH:
        break;

    default:
        return FALSE;
    }

    return TRUE;
}

DWORD GetViewstate(PVOID *context, DWORD argc, PCHAR argv[], PCHAR *result)
{
    PUSER_DATA  user;

    if (*context == NULL)
    {
        *result = NULL;
        return 0;
    }

    user = *context;

    *result = user->viewstate;

    return 0;
}

DWORD ResponseFilter(PVOID *context, DWORD argc, PCHAR argv[], PCHAR packet, ULONG_PTR len, ULONG_PTR sequence)
{
    PCHAR       buffer  = packet;
    DWORD       i       = 0;
    PUSER_DATA  user    = NULL;

    if (*context == NULL)
    {
        *context = calloc(1, sizeof(USER_DATA));

        if (NULL == *context)
        {
            return -1;
        }
    }

    user = *context;

    if (sequence == 0)
    {
        user->state = UserStateFindViewState;
        user->viewstate[0] = '\0';
    }

    //
    // fastpath for when we don't have any work to do
    //
    if (user->state == UserStateFinished)
    {
        return WCATRESULT_SUCCESS;
    }

    while (len && *buffer)
    {
        switch (user->state)
        {
            case UserStateFindViewState:
                if (*buffer == '_')
                {
                    if (len < VIEWSTATE_LEN)
                    {
                        return WCATRESULT_MORE_DATA_NEEDED;
                    }

                    if (_strnicmp(buffer, VIEWSTATE, VIEWSTATE_LEN) == 0)
                    {
                        len     -= VIEWSTATE_LEN-1;
                        buffer  += VIEWSTATE_LEN-1;

                        user->state = UserStateFindValue;
                    }
                }

                break;
            case UserStateFindValue:
                if (*buffer == 'v' || *buffer == 'V')
                {
                    if (len < 5)
                    {
                        return WCATRESULT_MORE_DATA_NEEDED;
                    }

                    if (_strnicmp(buffer, "value", 5) == 0)
                    {
                        len     -= 4;
                        buffer  += 4;

                        user->state = UserStateFindEqual;
                    }
                }
                break;
            case UserStateFindEqual:
                if (*buffer == '=')
                {
                    user->state = UserStateFindFirstQuote;
                }

                break;
            case UserStateFindFirstQuote:
                if (*buffer == '"')
                {
                    user->state = UserStateGetValue;
                }

                break;
            case UserStateGetValue:
                i = 0;

                for (i=0; len && *buffer && *buffer != '"'; i++, len--, buffer++)
                {
                    if (i > MAX_VIEWSTATE)
                    {
                        return WCATRESULT_ERROR;
                    }

                    user->viewstate[i] = *buffer;
                }

                if (len == 0 || *buffer != '"')
                {
                    return WCATRESULT_MORE_DATA_NEEDED;
                }

                user->viewstate[i] = '\0';

                user->state = UserStateFinished;
                break;
            case UserStateFinished:
                break;
            default:
                break;
        }

        len--; buffer++;
    }

    return WCATRESULT_SUCCESS;
}
