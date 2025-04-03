using Fish_Manage.Models;
using Microsoft.AspNetCore.Mvc;

namespace ChatbotAPI.Controllers
{
    [Route("api/chatbot")]
    [ApiController]
    public class ChatbotController : ControllerBase
    {
        private static readonly Dictionary<string, string> responses = new Dictionary<string, string>
        {
            { "What types of fish do you sell?", "We sell a variety of fish including FreshWater and SaltWater Fish" },
            { "Do you sell live fish or only frozen fish?", "We only sell  live fish  and we are plaaning to sell frozen fish so keep in touch with us." },
            { "What are your store hours?", "Our store is open from 9 AM to 8 PM every day." },
            { "Do you offer delivery services?", "Yes, we provide home delivery for all orders." },
            { "Where is your store located?", "Our store is located at FPT Plaza 1, Ngu Hanh Son, Da Nang." },

            { "How do I place an order on your website?", "You can place an order by adding fish to your cart and proceeding to checkout." },
            { "What payment methods do you accept?", "We accept MOMO, VNPay and cash on delivery." },
            { "Can I pay on delivery?", "Yes, cash on delivery is available for local orders." },
            { "Do you offer bulk discounts?", "Yes, we provide discounts for bulk purchases. Contact us for details." },
            { "Is there a minimum order requirement?", "Absolutely no for suer, And just be free for buying our products." },

            { "How long does delivery take?", "Delivery typically takes 2-3 business days." },
            { "Do you deliver nationwide?", "Currently, we only deliver within the state." },
            { "How is the fish packaged for delivery?", "All fish are packed in insulated containers with saltwater or freshwater to keep the fish available to your home" },
            { "What happens if my order arrives late or is incorrect?", "Please contact customer support, and we will resolve the issue immediately." },

            //{ "Can I return a fish if I am not satisfied?", "We accept returns only if the fish is spoiled or damaged upon arrival." },
            //{ "What is your refund policy?", "Refunds are processed within 5-7 business days after verification." },
            { "How do I report a problem with my order?", "You can report an issue by contacting our support team via email or phone." },
            //{ "Do you replace damaged or spoiled fish?", "Yes, we offer replacements for damaged or spoiled fish." },
            { "How long does it take to process a refund?", "Refunds typically take 5-7 business days to process." },

        
            //{ "Do you sell organic or wild-caught fish?", "Yes, we offer both organic farm-raised and wild-caught fish." },
            //{ "Can I request a specific cut of fish?", "Yes, we can prepare the fish in the cut you prefer." },
            //{ "How should I store the fish after purchase?", "Keep fish refrigerated at or below 40°F, or freeze for long-term storage." },
            //{ "Do you offer pre-seasoned or marinated fish?", "Yes, we offer marinated fish with different seasoning options." },

            //{ "What’s the best way to cook [specific fish]?", "It depends on the fish type! Let us know which one you're interested in." },
            //{ "Do you provide cooking tips or recipes?", "Yes, we offer recipe suggestions on our website." },
            //{ "How long should I cook fish to ensure it's safe to eat?", "Fish should be cooked to an internal temperature of 145°F." },
            //{ "Can I eat your fish raw for sushi?", "Yes, our sushi-grade fish is safe to eat raw." },
            //{ "What fish is best for grilling, frying, or baking?", "Salmon is great for grilling, cod is excellent for frying, and halibut is perfect for baking." },

            { "Where do you source your fish from?", "Our fish are sourced from sustainable fisheries worldwide." },
            { "Are your fish farm-raised or wild-caught?", "We offer both farm-raised and wild-caught options." },
            //{ "Do you sell sustainably sourced seafood?", "Yes, we are committed to selling only sustainably sourced seafood." },
            //{ "Do you have any certifications (e.g., MSC, ASC)?", "Yes, our fish are certified by MSC and ASC." },
            //{ "What steps do you take to ensure freshness?", "We ensure freshness by quick-freezing and using cold-chain logistics." }
        };

        private static readonly List<string> questionList = new List<string>(responses.Keys);

        [HttpGet("questions")]
        public IActionResult GetQuestions()
        {
            return Ok(new { questions = questionList });
        }

        [HttpPost("ask")]
        public IActionResult GetResponse([FromBody] ChatRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Message))
            {
                return BadRequest(new { response = "Invalid request! Please select a question from the list." });
            }

            if (responses.TryGetValue(request.Message, out string answer))
            {
                return Ok(new { response = answer });
            }

            return Ok(new { response = "I'm not sure how to respond to that question.Please contact our mail or phoneNumber" });
        }
    }
}
